using System.Collections.Generic;
using System.Threading.Tasks;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

using MorseCode.ITask;

namespace CartaCore.Persistence
{
    /// <summary>
    /// Represents a database context for accessing the Carta DynamoDB table
    /// </summary>
    public class DynamoDbContext : INoSqlDbContext
    {
        /// <summary>
        /// Primary key name
        /// </summary>
        const string PRIMARY_KEY = "PK";
        /// <summary>
        /// Sorty key name
        /// </summary>
        const string SORT_KEY = "SK";

        /// <summary>
        /// Gets or sets the DynamoDb client through which calls are made
        /// </summary>
        /// <value>The DynamoDb client</value>
        protected AmazonDynamoDBClient Client { get; set; }

        /// <summary>
        /// Gets or sets the DynamoDb table name
        /// </summary>
        /// <value>The DynamoDb table</value>
        protected string TableName { get; set; }

        /// <summary>
        /// Gets or sets the DynamoDb table
        /// </summary>
        /// <value>The DynamoDb table</value>
        protected Table DbTable { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public DynamoDbContext(string awsAccessKey, string awsSecretKey, string tableName)
        {
            // Initialize DynamoDb client
            Client = new AmazonDynamoDBClient(
                awsAccessKey,
                awsSecretKey,
                Amazon.RegionEndpoint.USEast2);

            // Load the DynamoDB table
            TableName = tableName;
            DbTable = Table.LoadTable(Client, TableName);
        }

        /// <summary>
        /// Save the given document string under the given key asynchronously.
        /// </summary>
        /// <param name="partitionKey">The partition key of the of document</param>
        /// <param name="sortKey">The sort key of the of document</param>
        /// <param name="docId">The document ID</param>
        /// <param name="docString">The document string</param>
        /// <returns>A unique document identifier</returns>
        public async ITask<string> SaveDocumentStringAsync
        (
            string partitionKey,
            string sortKey,
            string docId,
            string docString
        )
        {
            Document item = Document.FromJson(docString);
            item.Add(PRIMARY_KEY, partitionKey);
            item.Add(SORT_KEY, sortKey);
            await DbTable.PutItemAsync(item);
            return docId;
        }

        /// <summary>
        /// Load the stored document for the given keys asynchronously.
        /// </summary>
        /// <param name="partitionKey">The partition key of the of document</param>
        /// <param name="sortKey">The sort key of the of document</param>
        /// <returns>A document string</returns>
        public async ITask<string> LoadDocumentStringAsync(string partitionKey, string sortKey)
        {
            Task<Document> task = DbTable.GetItemAsync(partitionKey, sortKey);
            Document item = await task;
            if (item is null)
            {
                return null;
            }
            else
            {
                item.Remove(PRIMARY_KEY);
                item.Remove(SORT_KEY);
                return item.ToJson();
            }
        }

        /// <summary>
        /// Load all of the documents for the given keys asynchronously.
        /// </summary>
        /// <param name="partitionKey">The partition key of the of document</param>
        /// <param name="sortKey">The sort key of the of document</param>
        /// <returns>A list of document string</returns>
        public async ITask<List<string>> LoadDocumentStringsAsync(string partitionKey, string sortKey)
        {
            // Define query request
            QueryRequest request = new QueryRequest
            {
                TableName = TableName,
                KeyConditionExpression = "PK = :v_partitionKey and begins_with(SK, :v_sortKey)",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":v_partitionKey", new AttributeValue {S=partitionKey}},
                    {":v_sortKey", new AttributeValue {S=sortKey }}
                }
            };

            // Run query
            QueryResponse response = await Client.QueryAsync(request);

            // Parse query into a list of document strings and return
            List<string> docStringList = new() { };
            if (response == null)
            {
                return docStringList;
            }
            else
            {       
                foreach (Dictionary<string, AttributeValue> item in response.Items)
                {
                    Document itemRead = Document.FromAttributeMap(item);
                    itemRead.Remove(PRIMARY_KEY);
                    itemRead.Remove(SORT_KEY);
                    docStringList.Add(itemRead.ToJson());
                }
                return docStringList;
            }
        }

        /// <summary>
        /// Deletes the document stored under the given key asynchronously.
        /// </summary>
        /// <param name="partitionKey">The partition key of the of document</param>
        /// <param name="sortKey">The sort key of the of document</param>
        public async ITask DeleteDocumentStringAsync(string partitionKey, string sortKey)
        {
            Task<Document> taskDeleted = DbTable.DeleteItemAsync(partitionKey, sortKey);
            await taskDeleted;
        }

    }
}

