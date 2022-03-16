using Amazon;
using Amazon.DynamoDBv2;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Reflection;
using System.Linq;
using CartaCore.Persistence;
using CartaWeb.Models.Data;
using CartaWeb.Models.Options;

namespace CartaWeb.Models.DocumentItem
{

    /// <summary>
    /// Contains methods needed to write and read JSON documents to and from a NoSQL database.
    /// </summary>
    public class Persistence
    {

        /// <summary>
        /// The NoSQL DB context used to read and write to the database.
        /// </summary>
        private readonly INoSqlDbContext _noSqlDbContext;

        private readonly AwsCdkOptions _awsCdkOptions;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="noSqlDbContext">The NoSQL DB context used to read and write to the database.</param>
        public Persistence(INoSqlDbContext noSqlDbContext)
        {
            _noSqlDbContext = noSqlDbContext;
        }

        public Persistence(INoSqlDbContext noSqlDbContext, AwsCdkOptions awsCdkOptions)
        {
            _noSqlDbContext = noSqlDbContext;
            _awsCdkOptions = awsCdkOptions;
        }

        /// <summary>
        /// Retrieves the secrets db context
        /// </summary>
        /// <param name="user">The user claims needed for reading secrets.</param>
        /// <returns>The secrets db context.</returns>
        private INoSqlDbContext GetSecretsNoSqlDbContext(ClaimsPrincipal user)
        {
            // Initialize DynamoDb client
            AmazonDynamoDBClient client;
            RegionEndpoint regionEndpoint = Amazon.RegionEndpoint.GetBySystemName(_awsCdkOptions.RegionEndpoint);
            if (_awsCdkOptions.AccessKey is null)
                client = new AmazonDynamoDBClient(regionEndpoint);
            else
                client = new AmazonDynamoDBClient(
                    _awsCdkOptions.AccessKey,
                    _awsCdkOptions.SecretKey,
                    regionEndpoint);
            return new DynamoDbSecretsContext(client, _awsCdkOptions.SecretsDynamoDBTable, new UserInformation(user).Id);
        }

        /// <summary>
        /// Updates an item's secret values from the secrets store
        /// </summary>
        /// <param name="item">The document item to update.</param>
        /// <param name="user">The user claims needed for reading secrets.</param>
        /// <returns>The document item with secrets updated.</returns>
        private async Task<Item> MergeItemSecretsAsync(Item item, ClaimsPrincipal user)
        {
            INoSqlDbContext secretsNoSqlDbContext = GetSecretsNoSqlDbContext(user);
            DbDocument dbDocument =
                await secretsNoSqlDbContext.ReadDocumentAsync(new UserInformation(user).Id, item.GetSortKey());
            if (dbDocument is null) return item;
            else
            {
                Item secretItem =
                    (Item)JsonSerializer.Deserialize(dbDocument.SecretJsonString, item.GetType(), Item.ReadJsonOptions);
                foreach (PropertyInfo property in secretItem.GetType().GetProperties())
                {
                    if (property.GetCustomAttribute<SecretAttribute>() is not null)
                    {
                        Console.WriteLine($"Setting property {property.Name}");
                        property.SetValue(item, property.GetValue(secretItem));
                    }                 
                }
                return item;
            }       
        }

        /// <summary>
        /// Reads a database item from the database.
        /// </summary>
        /// <param name="item">The document item, with partition and sort key identifiers set.</param>
        /// <returns>The document item read from the database.</returns>
        public async Task<Item> LoadItemAsync(Item item)
        {
            DbDocument dbDocument = await _noSqlDbContext.ReadDocumentAsync(item.GetPartitionKey(), item.GetSortKey());
            if (dbDocument is null) return null;
            else return (Item)JsonSerializer.Deserialize(dbDocument.JsonString, item.GetType(), Item.ReadJsonOptions);
        }

        /// <summary>
        /// Reads a database item from the database.
        /// </summary>
        /// <param name="item">The document item, with partition and sort key identifiers set.</param>
        /// <param name="user">The user claims needed for reading secrets.</param>
        /// <returns>The document item read from the database.</returns>
        public async Task<Item> LoadItemAsync(Item item, ClaimsPrincipal user)
        {
            Item readItem = await LoadItemAsync(item);
            Item mergedItem = await MergeItemSecretsAsync(readItem, user);
            return mergedItem;
        }

        /// <summary>
        /// Reads database items from the database.
        /// </summary>
        /// <param name="item">The document item, with partition key identifier set.</param>
        /// <returns>A list of document items read from the database.</returns>
        public async Task<IEnumerable<Item>> LoadItemsAsync(Item item)
        {
            IAsyncEnumerable<DbDocument> dbDocuments = _noSqlDbContext
                .ReadDocumentsAsync(item.GetPartitionKey(), item.SortKeyPrefix);
            List<Item> items = new() { };
            await foreach (DbDocument dbDocument in dbDocuments)
                items.Add((Item)JsonSerializer.Deserialize(dbDocument.JsonString, item.GetType(), Item.ReadJsonOptions));
            return items;
        }

        /// <summary>
        /// Reads database items from the database.
        /// </summary>
        /// <param name="item">The document item, with partition key identifier set.</param>
        /// <param name="user">The user claims needed for reading secrets.</param>
        /// <returns>A list of document items read from the database.</returns>
        public async Task<IEnumerable<Item>> LoadItemsAsync(Item item, ClaimsPrincipal user)
        {
            IAsyncEnumerable<DbDocument> dbDocuments = _noSqlDbContext
                .ReadDocumentsAsync(item.GetPartitionKey(), item.SortKeyPrefix);
            List<Item> items = new() { };
            await foreach (DbDocument dbDocument in dbDocuments)
            {
                Item readItem =
                    (Item)JsonSerializer.Deserialize(dbDocument.JsonString, item.GetType(), Item.ReadJsonOptions);
                Item mergedItem = await MergeItemSecretsAsync(readItem, user);
                items.Add(mergedItem);
            }

            return items;
        }

        /// <summary>
        /// Writes a database document to the database.
        /// </summary>
        /// <param name="dbDocument">The database document.</param>
        /// <returns>true if the write operation completed successfully, else false.</returns>
        public async Task<bool> WriteDbDocumentAsync(DbDocument dbDocument)
        {
            return await _noSqlDbContext.WriteDocumentAsync(dbDocument);
        }

        /// <summary>
        /// Writes a database document to the database.
        /// </summary>
        /// <param name="dbDocument">The database document.</param>
        /// <param name="user">The user claims needed for persisting secrets.</param>
        /// <returns>true if the write operation completed successfully, else false.</returns>
        public async Task<bool> WriteDbDocumentAsync(DbDocument dbDocument, ClaimsPrincipal user)
        {
            await GetSecretsNoSqlDbContext(user).WriteDocumentAsync(dbDocument);
            return await WriteDbDocumentAsync(dbDocument);
        }

        /// <summary>
        /// Writes a list of database documents to the database.
        /// </summary>
        /// <param name="dbDocuments">The database documents.</param>
        /// <returns>true if the write operations all completed successfully, else false.</returns>
        public async Task<bool> WriteDbDocumentsAsync(IEnumerable<DbDocument> dbDocuments)
        {
            return await _noSqlDbContext.WriteDocumentsAsync(dbDocuments);
        }

        /// <summary>
        /// Writes a list of database documents to the database.
        /// </summary>
        /// <param name="dbDocuments">The database documents.</param>
        /// <param name="user">The user claims needed for persisting secrets.</param>
        /// <returns>true if the write operations all completed successfully, else false.</returns>
        public async Task<bool> WriteDbDocumentsAsync(IEnumerable<DbDocument> dbDocuments, ClaimsPrincipal user)
        {
            INoSqlDbContext secretsNoSqlDbContext = GetSecretsNoSqlDbContext(user);
            foreach (DbDocument dbDocument in dbDocuments)
            {
                if (dbDocument.SecretJsonString is not null)
                    await secretsNoSqlDbContext.WriteDocumentAsync(dbDocument);                  
            }
            return await _noSqlDbContext.WriteDocumentsAsync(dbDocuments);
        }
    }
}
