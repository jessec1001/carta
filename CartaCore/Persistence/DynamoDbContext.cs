using System.Collections.Generic;
using System.Threading.Tasks;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

using MorseCode.ITask;

using NUlid;

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
        /// Sort key name
        /// </summary>
        const string SORT_KEY = "SK";
        /// <summary>
        /// Sort key name
        /// </summary>
        const string ID_FIELD = "id";

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
        /// Constructor for local instance
        /// </summary>
        public DynamoDbContext(AmazonDynamoDBClient client, string tableName)
        {
            // Set the DynamoDB client
            Client = client;

            // Load the DynamoDB table
            TableName = tableName;
            DbTable = Table.LoadTable(Client, TableName);
        }

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

        /// <inheritdoc />
        public async ITask<string> CreateDocumentStringAsync
        (
            string partitionKey,
            string sortKeyPrefix,
            string docString
        )
        {
            // Create the item and add keys
            string docId = Ulid.NewUlid().ToString();
            Document item = Document.FromJson(docString);
            item.Add(PRIMARY_KEY, partitionKey);
            item.Add(SORT_KEY, sortKeyPrefix + docId);
            item.Add(ID_FIELD, docId);

            // Define an expression to ensure that the item does not get overwritten
            Expression expression = new Expression();
            expression.ExpressionStatement = "attribute_not_exists(" + PRIMARY_KEY + ")";

            // Put the item
            try
            {
                await DbTable.PutItemAsync(item, new PutItemOperationConfig { ConditionalExpression = expression });
                return docId;
            }
            catch (ConditionalCheckFailedException e)
            {
                return null;
            }
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public async ITask<List<string>> LoadDocumentStringsAsync(string partitionKey, string sortKeyPrefix)
        {
            // Define query request
            QueryRequest request = new QueryRequest
            {
                TableName = TableName,
                KeyConditionExpression = "PK = :v_partitionKey and begins_with(SK, :v_sortKey)",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":v_partitionKey", new AttributeValue {S=partitionKey}},
                    {":v_sortKey", new AttributeValue {S=sortKeyPrefix }}
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

        /// <inheritdoc />
        public async ITask<bool> UpdateDocumentStringAsync
        (
            string partitionKey,
            string sortKey,
            string docString
        )
        {
            // Create the item and add keys
            Document item = Document.FromJson(docString);
            item.Add(PRIMARY_KEY, partitionKey);
            item.Add(SORT_KEY, sortKey);

            // Define an expression to ensure that the item does not get overwritten
            Expression expression = new Expression();
            expression.ExpressionStatement = "attribute_exists(" + PRIMARY_KEY + ")";

            // Put the item
            try
            {
                await DbTable.PutItemAsync(item, new PutItemOperationConfig { ConditionalExpression = expression });
                return true;
            }
            catch (ConditionalCheckFailedException e)
            {
                return false;
            }
        }

        /// <inheritdoc />
        public async ITask SaveDocumentStringAsync
        (
            string partitionKey,
            string sortKey,
            string docString
        )
        {
            // Create the item and add keys
            Document item = Document.FromJson(docString);
            item.Add(PRIMARY_KEY, partitionKey);
            item.Add(SORT_KEY, sortKey);
            await DbTable.PutItemAsync(item);
        }

        /// <inheritdoc />
        public async ITask<bool> DeleteDocumentStringAsync(string partitionKey, string sortKey)
        {
            // Define an expression to check that the item exists before it gets deleted
            Expression expression = new Expression();
            expression.ExpressionStatement = "attribute_exists(" + PRIMARY_KEY + ")";

            // Delete the item
            try
            {
                await DbTable.DeleteItemAsync
                (
                    partitionKey,
                    sortKey,
                    new DeleteItemOperationConfig { ConditionalExpression = expression }
                );
                return true;
            }
            catch (ConditionalCheckFailedException e)
            {
                return false;
            }
        }
    }
}

