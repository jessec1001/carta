using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;

using NUnit.Framework;

using CartaCore.Persistence;

namespace CartaTest
{
    /// <summary>
    /// Tests the DynamoDbContext object.
    /// </summary>
    [TestFixture]
    public class DynamoDbContextTests
    {
        /// <summary>
        /// The graph generated to test on.
        /// </summary>
        protected INoSqlDbContext NoSqlDbContext;

        /// <summary>
        /// Helper method to get DynamoDB local hostname
        /// </summary>
        private static string GetDynamoDbLocalHostName()
        {
            return Environment.GetEnvironmentVariable("DYNAMODB_LOCAL_HOSTNAME") is null ?
                "localhost" :
                Environment.GetEnvironmentVariable("DYNAMODB_LOCAL_HOSTNAME");
        }

        /// <summary>
        /// Helper method for Setup to create the table
        /// </summary>
        private async Task CreateTable(AmazonDynamoDBClient client, string tableName)
        {
            CreateTableRequest  request = new CreateTableRequest
            {
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = "PK",
                        KeyType = "HASH" //Partition key
                    },
                    new KeySchemaElement
                    {
                        AttributeName = "SK",
                        KeyType = "RANGE" //Sort key
                    }
                },
                AttributeDefinitions = new List<AttributeDefinition>()
                {
                    new AttributeDefinition
                    {
                        AttributeName = "PK",
                        AttributeType = ScalarAttributeType.S
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "SK",
                        AttributeType = ScalarAttributeType.S
                    }
                },
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 5,
                    WriteCapacityUnits = 6
                },
                TableName = tableName
            };
            await client.CreateTableAsync(request);
        }

        /// <summary>
        /// Sets up the test fixture.
        /// </summary>
        [SetUp]
        public async Task Setup()
        {
            string localServiceUrl = "http://" + GetDynamoDbLocalHostName() + ":8000";
            string tableName = "CartaRegressionTests";

            // Create the DynamoDB client
            AmazonDynamoDBConfig clientConfig = new AmazonDynamoDBConfig { };
            clientConfig.ServiceURL = localServiceUrl;
            // Note: the AWS access and secret key must be set to (any) non-empty strings even though they are
            // not real credentials, else the Amazon SDK will throw exceptions
            AmazonDynamoDBClient client = new AmazonDynamoDBClient
                (new BasicAWSCredentials("LocalAccessKey", "LocalSecretKey"), clientConfig);

            // Create test table if it does not exist yet
            ListTablesResponse tablesResponse = await client.ListTablesAsync();
            List<string> tableNames = tablesResponse.TableNames;
            if (!tableNames.Contains(tableName)) await CreateTable(client, tableName); 

            // Check if table is available before continuing
            int i = 0;
            bool isTableAvailable = false;
            while (!isTableAvailable)
            {
                if (i >= 5)
                {
                    Assert.Fail("ERROR! Table creation has timed out");
                    return;
                }       
                i++;
                DescribeTableResponse tableStatus = await client.DescribeTableAsync(tableName);
                isTableAvailable = tableStatus.Table.TableStatus.ToString() == "ACTIVE";
                Thread.Sleep(5000);
            }

            // Initialize the DynamoDB instance
            NoSqlDbContext = new DynamoDbContext(client, tableName);
        }

        /// <summary>
        /// Tests that a JSON string can be saved, loaded and deleted
        /// </summary>
        [Test]
        public async Task TestSaveLoadUpdateDeleteDocumentStringAsync()
        {
            // Test save new and load
            string inJsonString = "{\"field1\":\"value1\"}";
            await NoSqlDbContext.SaveDocumentStringAsync("pk1", "sk1", inJsonString);
            string readJsonString = await NoSqlDbContext.LoadDocumentStringAsync("pk1", "sk1");
            Assert.AreEqual(inJsonString, readJsonString);

            // Test save update
            inJsonString = "{\"field1\":\"value1b\"}";
            await NoSqlDbContext.SaveDocumentStringAsync("pk1", "sk1", inJsonString);
            readJsonString = await NoSqlDbContext.LoadDocumentStringAsync("pk1", "sk1");
            Assert.AreEqual(inJsonString, readJsonString);

            // Test delete
            bool deleted = await NoSqlDbContext.DeleteDocumentStringAsync("pk1", "sk1");
            Assert.IsTrue(deleted);
            readJsonString = await NoSqlDbContext.LoadDocumentStringAsync("pk1", "sk1");
            Assert.IsNull(readJsonString);
        }

        /// <summary>
        /// Tests that multiple document strings can be read
        /// </summary>
        [Test]
        public async Task TestLoadDocumentStringsAsync()
        {
            // Test that no records are returned if records do not exist
            List<string> readJsonStrings = await NoSqlDbContext.LoadDocumentStringsAsync("pk1", "sk1");
            Assert.AreEqual(0, readJsonStrings.Count);

            // Save 2 document strings under the same primary key but different sort keys
            string inJsonString = "{\"field1\":\"value1\"}";
            await NoSqlDbContext.SaveDocumentStringAsync("pk1", "sk1", inJsonString);
            await NoSqlDbContext.SaveDocumentStringAsync("pk1", "sk2", inJsonString);

            // Test that 2 document strings are returned
            readJsonStrings = await NoSqlDbContext.LoadDocumentStringsAsync("pk1", "sk");
            Assert.AreEqual(2, readJsonStrings.Count);

            // Cleanup
            await NoSqlDbContext.DeleteDocumentStringAsync("pk1", "sk1");
            await NoSqlDbContext.DeleteDocumentStringAsync("pk1", "sk2");
        }

        /// <summary>
        /// Tests that a JSON string is created successfully with an unique Id
        /// </summary>
        [Test]
        public async Task TestCreateDocumentStringAsync()
        {
            // Test create for record that does not exist
            string inJsonString = "{\"field1\":\"value1\"}";
            string docId = await NoSqlDbContext.CreateDocumentStringAsync("pk1", "sk", inJsonString);
            string readJsonString = await NoSqlDbContext.LoadDocumentStringAsync("pk1", "sk" + docId);
            Assert.IsNotNull(docId);
            Assert.AreEqual("{\"field1\":\"value1\",\"id\":\"" + docId + "\"}", readJsonString);

            // Cleanup
            await NoSqlDbContext.DeleteDocumentStringAsync("pk1", "sk" + docId);
        }

        /// <summary>
        /// Tests that a JSON string is updated successfully, but only if it exists
        /// </summary>
        [Test]
        public async Task TestUpdateDocumentStringAsync()
        {
            // Test update for record that does not exist
            string inJsonString = "{\"field1\":\"value1\"}";
            bool updated = await NoSqlDbContext.UpdateDocumentStringAsync("pk1", "sk-not-exist", inJsonString);
            Assert.IsFalse(updated);

            // Test update for record that does exist
            string docId = await NoSqlDbContext.CreateDocumentStringAsync("pk1", "sk", inJsonString);
            inJsonString = "{\"field1b\":\"value1b\"}";
            updated = await NoSqlDbContext.UpdateDocumentStringAsync("pk1", "sk" + docId, inJsonString);
            Assert.IsTrue(updated);
            string readJsonString = await NoSqlDbContext.LoadDocumentStringAsync("pk1", "sk" + docId);
            Assert.AreEqual(inJsonString, readJsonString);

            // Cleanup
            await NoSqlDbContext.DeleteDocumentStringAsync("pk1", "sk" + docId);
        }

        /// <summary>
        /// Tests that delete of a non existing record returns false
        /// </summary>
        [Test]
        public async Task TestNotExistDeleteDocumentStringAsync()
        {
            // Test delete
            bool deleted = await NoSqlDbContext.DeleteDocumentStringAsync("pkx", "sky");
            Assert.IsFalse(deleted);
        }

    }
}
