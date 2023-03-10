using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;

using NUnit.Framework;

using CartaCore.Persistence;
using System.Net.Sockets;

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
        private static async Task CreateTable(AmazonDynamoDBClient client, string tableName)
        {
            CreateTableRequest request = new()
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
        [OneTimeSetUp]
        public async Task Setup()
        {
            // Test if local dynamo DB instance is up and running, and if not, a SocketException will be thrown,
            // causing the tests to fail immediately (rather than waiting for a long time before giving up on
            // the connection).
            TcpClient tcpClient = new TcpClient(GetDynamoDbLocalHostName(), 8000);

            string localServiceUrl = "http://" + GetDynamoDbLocalHostName() + ":8000";
            string tableName = "CartaRegressionTests";

            // Create the DynamoDB client
            AmazonDynamoDBConfig clientConfig = new() { };
            clientConfig.ServiceURL = localServiceUrl;
            // Note: the AWS access and secret key must be set to (any) non-empty strings even though they are
            // not real credentials, else the Amazon SDK will throw exceptions
            AmazonDynamoDBClient client = new(new BasicAWSCredentials("LocalAccessKey", "LocalSecretKey"), clientConfig);

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
            DbDocument dbDocument = new("pk#1", "sk#1", inJsonString, DbOperationType.Save);
            bool isSaved = await NoSqlDbContext.WriteDocumentAsync(dbDocument);
            Assert.IsTrue(isSaved);
            DbDocument dbDocumentRead = await NoSqlDbContext.ReadDocumentAsync("pk#1", "sk#1");
            Assert.AreEqual(inJsonString, dbDocumentRead.JsonString);

            // Test save update
            inJsonString = "{\"field1\":\"value1b\"}";
            dbDocument = new DbDocument("pk#1", "sk#1", inJsonString, DbOperationType.Save);
            isSaved = await NoSqlDbContext.WriteDocumentAsync(dbDocument);
            Assert.IsTrue(isSaved);
            dbDocumentRead = await NoSqlDbContext.ReadDocumentAsync("pk#1", "sk#1");
            Assert.AreEqual(inJsonString, dbDocumentRead.JsonString);

            // Test delete
            bool deleted =
                await NoSqlDbContext.WriteDocumentAsync(new DbDocument("pk#1", "sk#1", DbOperationType.Delete));
            Assert.IsTrue(deleted);
            dbDocumentRead = await NoSqlDbContext.ReadDocumentAsync("pk#1", "sk#1");
            Assert.IsNull(dbDocumentRead);
        }

        /// <summary>
        /// Tests that multiple document strings can be read
        /// </summary>
        [Test]
        public async Task TestLoadDocumentStringsAsync()
        {
            // Test that no records are returned if records do not exist
            IAsyncEnumerable<DbDocument> readDocs = NoSqlDbContext.ReadDocumentsAsync("pk#1", "sk#");
            Assert.AreEqual(0, await readDocs.CountAsync());

            // Save 2 document strings under the same primary key but different sort keys
            string inJsonString = "{\"field1\":\"value1\"}";
            await NoSqlDbContext.WriteDocumentAsync(
                new DbDocument("pk#1", "sk#1", inJsonString, DbOperationType.Save));
            await NoSqlDbContext.WriteDocumentAsync(
                new DbDocument("pk#1", "sk#2", inJsonString, DbOperationType.Save));

            // Test that 2 document strings are returned
            readDocs = NoSqlDbContext.ReadDocumentsAsync("pk#1", "sk#");
            Assert.AreEqual(2, await readDocs.CountAsync());

            // Cleanup
            await NoSqlDbContext.WriteDocumentAsync(new DbDocument("pk#1", "sk#1", DbOperationType.Delete));
            await NoSqlDbContext.WriteDocumentAsync(new DbDocument("pk#1", "sk#2", DbOperationType.Delete));
        }

        /// <summary>
        /// Tests that a JSON string is created successfully with an unique Id
        /// </summary>
        [Test]
        public async Task TestCreateDocumentStringAsync()
        {
            // Test create for record that does not exist
            string inJsonString = "{\"field1\":\"value1\"}";
            DbDocument dbDocument = new("pk#1", "skcreate1#", inJsonString, DbOperationType.Create);
            bool isCreated = await NoSqlDbContext.WriteDocumentAsync(dbDocument);
            Assert.IsTrue(isCreated);
            IAsyncEnumerable<DbDocument> dbDocuments = NoSqlDbContext.ReadDocumentsAsync("pk#1", "skcreate1#");
            DbDocument dbDocumentRead = await dbDocuments.ElementAtAsync(0);
            Assert.AreEqual(inJsonString, dbDocumentRead.JsonString);

            // Cleanup
            await NoSqlDbContext.WriteDocumentAsync(new DbDocument("pk#1", "skcreate1#", DbOperationType.Delete));
        }

        /// <summary>
        /// Tests that a JSON string is updated successfully, but only if it exists
        /// </summary>
        [Test]
        public async Task TestUpdateDocumentStringAsync()
        {
            // Test update for record that does not exist
            string inJsonString = "{\"field1\":\"value1\"}";
            DbDocument dbDocument = new("pk#1", "sk-not-exist", inJsonString, DbOperationType.Update);
            bool updated = await NoSqlDbContext.WriteDocumentAsync(dbDocument);
            Assert.IsFalse(updated);

            // Test update for record that does exist
            inJsonString = "{\"field1\":\"value1\"}";
            dbDocument = new DbDocument("pk#1", "sk#1", inJsonString, DbOperationType.Save);
            await NoSqlDbContext.WriteDocumentAsync(dbDocument);
            inJsonString = "{\"field1b\":\"value1b\"}";
            dbDocument = new DbDocument("pk#1", "sk#1", inJsonString, DbOperationType.Update);
            updated = await NoSqlDbContext.WriteDocumentAsync(dbDocument);
            Assert.IsTrue(updated);
            DbDocument dbDocumentRead = await NoSqlDbContext.ReadDocumentAsync("pk#1", "sk#1");
            Assert.AreEqual(inJsonString, dbDocumentRead.JsonString);

            // Cleanup
            await NoSqlDbContext.WriteDocumentAsync(new DbDocument("pk#1", "sk#1", DbOperationType.Delete));
        }

        /// <summary>
        /// Tests that delete of a non existing record returns false
        /// </summary>
        [Test]
        public async Task TestNotExistDeleteDocumentStringAsync()
        {
            // Test delete
            bool deleted =
                await NoSqlDbContext.WriteDocumentAsync(new DbDocument("pk#x", "sky", DbOperationType.Delete));
            Assert.IsFalse(deleted);
        }

        /// <summary>
        /// Tests that read operations return false if used with a write instruction
        /// </summary>
        [Test]
        public async Task TestReadOperationWithWrite()
        {
            // Test single write
            bool isRead =
                await NoSqlDbContext.WriteDocumentAsync(new DbDocument("pk#x", "sky", null, DbOperationType.Read));
            Assert.IsFalse(isRead);

            // Test transaction write
            isRead =
                await NoSqlDbContext.WriteDocumentsAsync(
                    new List<DbDocument>() { new DbDocument("pk#x", "sky", null, DbOperationType.Read) });
            Assert.IsFalse(isRead);
        }

        /// <summary>
        /// Tests that the create, save, update, delete operations work when called with the transaction method
        /// </summary>
        [Test]
        public async Task TestCreateSaveUpdateDeleteSingleWriteTransactions()
        {
            // Test create of new item - should succeed
            string inJsonString = "{\"field1\":\"value1\"}";
            DbDocument dbDocument = new("pk#1", "skcreate2#", inJsonString, DbOperationType.Create);
            bool isCreated = await NoSqlDbContext.WriteDocumentsAsync(new List<DbDocument>() { dbDocument });
            Assert.IsTrue(isCreated);
            IAsyncEnumerable<DbDocument> dbDocuments = NoSqlDbContext.ReadDocumentsAsync("pk#1", "skcreate2#");
            DbDocument dbDocumentRead = await dbDocuments.ElementAtAsync(0);
            Assert.AreEqual(inJsonString, dbDocumentRead.JsonString);

            // Test save of new item - should succeed
            inJsonString = "{\"field1\":\"value1c\"}";
            dbDocument = new DbDocument("pk#1", "sk#2", inJsonString, DbOperationType.Save);
            bool isSaved = await NoSqlDbContext.WriteDocumentsAsync(new List<DbDocument>() { dbDocument });
            Assert.IsTrue(isSaved);
            dbDocumentRead = await NoSqlDbContext.ReadDocumentAsync("pk#1", "sk#2");
            Assert.AreEqual(inJsonString, dbDocumentRead.JsonString);

            // Test save of existing item - should succeed
            inJsonString = "{\"field1\":\"value1d\"}";
            dbDocument = new DbDocument("pk#1", "sk#2", inJsonString, DbOperationType.Save);
            isSaved = await NoSqlDbContext.WriteDocumentsAsync(new List<DbDocument>() { dbDocument });
            Assert.IsTrue(isSaved);
            dbDocumentRead = await NoSqlDbContext.ReadDocumentAsync("pk#1", "sk#2");
            Assert.AreEqual(inJsonString, dbDocumentRead.JsonString);

            // Test update of new item - should fail
            inJsonString = "{\"field1\":\"value1e\"}";
            dbDocument = new DbDocument("pk#1", "sk-not-exist", inJsonString, DbOperationType.Update);
            bool isUpdated = await NoSqlDbContext.WriteDocumentsAsync(new List<DbDocument>() { dbDocument });
            Assert.IsFalse(isUpdated);

            // Test update of existing item - should succeed
            inJsonString = "{\"field1\":\"value1f\"}";
            dbDocument = new DbDocument("pk#1", "sk#2", inJsonString, DbOperationType.Update);
            isUpdated = await NoSqlDbContext.WriteDocumentsAsync(new List<DbDocument>() { dbDocument });
            Assert.IsTrue(isUpdated);
            dbDocumentRead = await NoSqlDbContext.ReadDocumentAsync("pk#1", "sk#2");
            Assert.AreEqual(inJsonString, dbDocumentRead.JsonString);

            // Test delete of new item - should fail
            dbDocument = new DbDocument("pk#1", "sk#not-exist", DbOperationType.Delete);
            bool isDeleted = await NoSqlDbContext.WriteDocumentsAsync(new List<DbDocument>() { dbDocument });
            Assert.IsFalse(isDeleted);
            IAsyncEnumerable<DbDocument> readDocs = NoSqlDbContext.ReadDocumentsAsync("pk#1", "sk#");
            Assert.AreEqual(1, await readDocs.CountAsync());

            // Test delete of existing item - should succeed
            dbDocument = new DbDocument("pk#1", "sk#2", DbOperationType.Delete);
            isDeleted = await NoSqlDbContext.WriteDocumentsAsync(new List<DbDocument>() { dbDocument });
            Assert.IsTrue(isDeleted);
            readDocs = NoSqlDbContext.ReadDocumentsAsync("pk#1", "sk#");
            Assert.AreEqual(0, await readDocs.CountAsync());

            // Cleanup
            await NoSqlDbContext.WriteDocumentAsync(new DbDocument("pk#1", "skcreate2#", DbOperationType.Delete));
        }

        /// <summary>
        /// Tests that a sequence of write items run successfully as a transaction
        /// </summary>
        [Test]
        public async Task TestWriteTransaction()
        {
            // Create write items
            List<DbDocument> dbDocuments = new() { };
            string inJsonString = "{\"field1\":\"value1\"}";
            dbDocuments.Add(new DbDocument("pk#1", "sk#1", inJsonString, DbOperationType.Save));
            inJsonString = "{\"field1\":\"value1b\"}";
            dbDocuments.Add(new DbDocument("pk#1", "sk#2", inJsonString, DbOperationType.Save));

            // Check that the operation completes
            bool completed = await NoSqlDbContext.WriteDocumentsAsync(dbDocuments);
            Assert.IsTrue(completed);

            // Check that persisted values are as expected
            DbDocument dbDocumentRead = await NoSqlDbContext.ReadDocumentAsync("pk#1", "sk#1");
            Assert.AreEqual("{\"field1\":\"value1\"}", dbDocumentRead.JsonString);
            dbDocumentRead = await NoSqlDbContext.ReadDocumentAsync("pk#1", "sk#2");
            Assert.AreEqual("{\"field1\":\"value1b\"}", dbDocumentRead.JsonString);

            // Cleanup
            await NoSqlDbContext.WriteDocumentAsync(new DbDocument("pk#1", "sk#1", DbOperationType.Delete));
            await NoSqlDbContext.WriteDocumentAsync(new DbDocument("pk#1", "sk#2", DbOperationType.Delete));
        }

        /// <summary>
        /// Tests that a sequence of write items where one should fail, does not get completed as a transaction
        /// </summary>
        [Test]
        public async Task TestWriteTransactionFail()
        {
            // Create write items
            List<DbDocument> dbDocuments = new() { };
            string inJsonString = "{\"field1\":\"value1\"}";
            dbDocuments.Add(new DbDocument("pk#1", "sk#1", inJsonString, DbOperationType.Save));
            dbDocuments.Add(new DbDocument("pk#1", "sk-non-exist", DbOperationType.Delete));

            // Check that the operation does not complete
            bool completed = await NoSqlDbContext.WriteDocumentsAsync(dbDocuments);
            Assert.IsFalse(completed);

            // Check that no values have been persistied
            DbDocument dbDocumentRead = await NoSqlDbContext.ReadDocumentAsync("pk#1", "sk#1");
            Assert.IsNull(dbDocumentRead);
        }
    }
}
