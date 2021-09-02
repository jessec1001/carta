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
        /// Sort key name
        /// </summary>
        const string SORT_KEY = "SK";

        /// <summary>
        /// Gets or sets the DynamoDb client through which calls are made
        /// </summary>
        /// <value>The DynamoDb client</value>
        public AmazonDynamoDBClient Client { get; set; }

        /// <summary>
        /// Gets or sets the DynamoDb table name
        /// </summary>
        /// <value>The DynamoDb table</value>
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets the DynamoDb table
        /// </summary>
        /// <value>The DynamoDb table</value>
        public Table DbTable { get; set; }

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
        public DynamoDbContext
        (
            string awsAccessKey,
            string awsSecretKey,
            Amazon.RegionEndpoint regionEndpoint,
            string tableName
        )
        {
            // Initialize DynamoDb client
            if (awsAccessKey is null)
                Client = new AmazonDynamoDBClient(regionEndpoint);
            else
                Client = new AmazonDynamoDBClient(
                    awsAccessKey,
                    awsSecretKey,
                    regionEndpoint);

            // Load the DynamoDB table
            TableName = tableName;
            DbTable = Table.LoadTable(Client, TableName);
        }

        private static Document GetPutItem(DbDocument dbDocument)
        {
            Document item = Document.FromJson(dbDocument.JsonString);
            item.Add(PRIMARY_KEY, dbDocument.PartitionKey);
            item.Add(SORT_KEY, dbDocument.SortKey);
            return item;
        }

        private static string GetConditionalExpression(DbOperationEnumeration dbOperation)
        {
            switch (dbOperation)
            {
                case DbOperationEnumeration.Create: return "attribute_not_exists(" + PRIMARY_KEY + ")";
                case DbOperationEnumeration.Update: return "attribute_exists(" + PRIMARY_KEY + ")";
                case DbOperationEnumeration.Delete: return "attribute_exists(" + PRIMARY_KEY + ")";
            }
            return null;
        }

        /// <inheritdoc />
        public async ITask<DbDocument> ReadDocumentAsync(string partitionKey, string sortKey)
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
                return new DbDocument(partitionKey, sortKey, item.ToJson(), DbOperationEnumeration.Read);
            }
        }

        /// <inheritdoc />
        public async ITask<IEnumerable<DbDocument>> ReadDocumentsAsync(string partitionKey, string sortKeyPrefix)
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

            // Parse query into a list of documents and return
            List<DbDocument> dbDocuments= new() { };
            if (response == null)
            {
                return dbDocuments;
            }
            else
            {
                foreach (Dictionary<string, AttributeValue> item in response.Items)
                {
                    Document itemRead = Document.FromAttributeMap(item);
                    itemRead.Remove(PRIMARY_KEY);
                    itemRead.Remove(SORT_KEY);
                    dbDocuments.Add(
                        new DbDocument(partitionKey, sortKeyPrefix, itemRead.ToJson(), DbOperationEnumeration.Read));
                }
                return dbDocuments;
            }
        }

        /// <inheritdoc />
        public async ITask<bool> WriteDocumentAsync(DbDocument dbDocument)
        {
            if (dbDocument.DbOperation == DbOperationEnumeration.Read) return false;
            else if (dbDocument.DbOperation == DbOperationEnumeration.Delete)
            {
                Expression expression = new Expression();
                expression.ExpressionStatement = GetConditionalExpression(dbDocument.DbOperation);
                try
                {
                    await DbTable.DeleteItemAsync
                    (
                        dbDocument.PartitionKey,
                        dbDocument.SortKey,
                        new DeleteItemOperationConfig { ConditionalExpression = expression }
                    );
                    return true;
                }
                catch (ConditionalCheckFailedException)
                {
                    return false;
                }
            }
            else
            {
                Document item = GetPutItem(dbDocument);
                Expression expression = new Expression();
                expression.ExpressionStatement = GetConditionalExpression(dbDocument.DbOperation);
                try
                {
                    await DbTable.PutItemAsync(item, new PutItemOperationConfig { ConditionalExpression = expression });
                    return true;
                }
                catch (ConditionalCheckFailedException)
                {
                    return false;
                }
            }
        }

        /// <inheritdoc />
        public async ITask<bool> WriteDocumentsAsync(IEnumerable<DbDocument> dbDocuments)
        {
            // Build the list of transaction items
            List<TransactWriteItem> transactWriteItems = new() { };
            foreach(DbDocument dbDocument in dbDocuments)
            {
                TransactWriteItem transactWriteItem = new TransactWriteItem();
                if (dbDocument.DbOperation == DbOperationEnumeration.Read) return false;
                else if (dbDocument.DbOperation == DbOperationEnumeration.Delete)
                {
                    Delete delete = new Delete();
                    delete.Key = new Dictionary<string, AttributeValue>
                    {
                        {"PK", new AttributeValue {S=dbDocument.PartitionKey}},
                        {"SK", new AttributeValue {S=dbDocument.SortKey}}
                    };
                    delete.ConditionExpression = GetConditionalExpression(dbDocument.DbOperation);
                    delete.TableName = TableName;
                    transactWriteItem.Delete = delete;
                }
                else
                {
                    Document item = GetPutItem(dbDocument);
                    Put put = new Put();
                    put.Item = item.ToAttributeMap();
                    put.ConditionExpression = GetConditionalExpression(dbDocument.DbOperation);
                    put.TableName = TableName;
                    transactWriteItem.Put = put;
                }
                transactWriteItems.Add(transactWriteItem);
            }

            // Execute the transaction
            TransactWriteItemsRequest transactWriteItemsRequest = new TransactWriteItemsRequest();
            transactWriteItemsRequest.TransactItems = transactWriteItems;
            try
            {
                await Client.TransactWriteItemsAsync(transactWriteItemsRequest);
                return true;
            }
            catch (TransactionCanceledException)
            {
                return false;
            }
        }
    }
}

