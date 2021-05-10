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
        /// Tests that a JSON string has been persisted and can be read, updated and deleted
        /// </summary>
        [Test]
        public async Task TestSaveLoadUpdateDeleteDocumentStringAsync()
        {
            // Test save
            string inJsonString = "{\"field1\":\"value1\"}";
            string inDocId = "id1";
            string docId = await NoSqlDbContext.SaveDocumentStringAsync("pk1", "sk1", inDocId, inJsonString);
            Assert.AreEqual(inDocId, docId);

            // Test load
            string readJsonString = await NoSqlDbContext.LoadDocumentStringAsync("pk1", "sk1");
            Assert.AreEqual(inJsonString, readJsonString);

            // Test update
            inJsonString = "{\"field1\":\"value1b\"}";
            docId = await NoSqlDbContext.SaveDocumentStringAsync("pk1", "sk1", inDocId, inJsonString);
            Assert.AreEqual(inDocId, docId);
            readJsonString = await NoSqlDbContext.LoadDocumentStringAsync("pk1", "sk1");
            Assert.AreEqual(inJsonString, readJsonString);

            // Test delete
            await NoSqlDbContext.DeleteDocumentStringAsync("pk1", "sk1");
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
            List<string> readJsonStrings = await NoSqlDbContext.LoadDocumentStringsAsync("pk1", "sk");
            Assert.AreEqual(0, readJsonStrings.Count);

            // Save 2 document strings under the same primary key but different sort keys
            string inJsonString = "{\"field1\":\"value1\"}";
            string inDocId = "id1";
            await NoSqlDbContext.SaveDocumentStringAsync("pk1", "sk1", inDocId, inJsonString);
            await NoSqlDbContext.SaveDocumentStringAsync("pk1", "sk2", inDocId, inJsonString);

            // Test that 2 document strings are returned
            readJsonStrings = await NoSqlDbContext.LoadDocumentStringsAsync("pk1", "sk");
            Assert.AreEqual(2, readJsonStrings.Count);

            // Cleanup
            await NoSqlDbContext.DeleteDocumentStringAsync("pk1", "sk1");
            await NoSqlDbContext.DeleteDocumentStringAsync("pk1", "sk2");
        }

    }
}
