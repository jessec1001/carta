using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

namespace CartaCore.Persistence
{
    /// <summary>
    /// Represents a database context for accessing the Carta DynamoDB secrets table.
    /// </summary>
    public class DynamoDbSecretsContext : INoSqlDbContext
    {
        /// <summary>
        /// Primary key name
        /// </summary>
        const string PRIMARY_KEY = "UserId";
        /// <summary>
        /// Sort key name
        /// </summary>
        const string SORT_KEY = "Key";
        /// <summary>
        /// Time to live attribute
        /// </summary>
        const string TTL = "TTL";

        /// <summary>
        /// Gets or sets the DynamoDb client through which calls are made
        /// </summary>
        /// <value>The DynamoDb client</value>
        public AmazonDynamoDBClient Client { get; private init; }

        /// <summary>
        /// Gets or sets the DynamoDb table
        /// </summary>
        /// <value>The DynamoDb table</value>
        public Table DbTable { get; private init; }

        /// <summary>
        /// Gets or sets the user ID secrets are stored under
        /// </summary>
        /// <value>The user ID</value>
        public string UserId { get; private init; }

        /// <summary>
        /// Constructor with DynamoDB client
        /// </summary>
        public DynamoDbSecretsContext(AmazonDynamoDBClient client, string tableName, string userId)
        {
            // Set the DynamoDB client
            Client = client;

            // Load the DynamoDB table
            DbTable = Table.LoadTable(Client, tableName);

            // Set the user ID
            UserId = userId;
        }

        /// <inheritdoc />
        public async Task<DbDocument> ReadDocumentAsync(string userId, string sortKey)
        {
            Task<Document> task = DbTable.GetItemAsync(userId, sortKey);
            Document item = await task;
            if (item is null)
            {
                return null;
            }
            else
            {
                item.Remove(PRIMARY_KEY);
                item.Remove(SORT_KEY);
                return new DbDocument(userId, sortKey, null, item.ToJson(), DbOperationType.Read);
            }
        }

        /// <inheritdoc />
        public IAsyncEnumerable<DbDocument> ReadDocumentsAsync(string partitionKey, string sortKeyPrefix)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public async Task<bool> WriteDocumentAsync(DbDocument dbDocument)
        {
            if (dbDocument.Operation == DbOperationType.Read) return false;
            if (dbDocument.SecretJsonString is null) return false;
            else if (dbDocument.Operation == DbOperationType.Delete)
            {
                await DbTable.DeleteItemAsync(UserId, dbDocument.SortKey);
                return true;
            }
            else
            {
                Document item = Document.FromJson(dbDocument.SecretJsonString);
                item.Add(PRIMARY_KEY, UserId);
                item.Add(SORT_KEY, dbDocument.SortKey);
                item.Add(TTL, (new DateTimeOffset(DateTime.Now.Add(new TimeSpan(12, 0, 0)))).ToUnixTimeSeconds());
                await DbTable.PutItemAsync(item);
                return true;
            }
        }

        /// <inheritdoc />
        public Task<bool> WriteDocumentsAsync(IEnumerable<DbDocument> dbDocuments)
        {
            throw new NotSupportedException();
        }
    }
}

