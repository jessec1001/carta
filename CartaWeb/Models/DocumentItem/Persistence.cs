using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Claims;
using CartaCore.Persistence;
using CartaWeb.Models.Data;

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

        private readonly IUserSecretsContext _userSecretsContext;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="noSqlDbContext">The NoSQL DB context used to read and write to the database.</param>
        public Persistence(INoSqlDbContext noSqlDbContext)
        {
            _noSqlDbContext = noSqlDbContext;
        }

        public Persistence(INoSqlDbContext noSqlDbContext, IUserSecretsContext userSecretsContext)
        {
            _noSqlDbContext = noSqlDbContext;
            _userSecretsContext = userSecretsContext;
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
            else return (Item)JsonSerializer.Deserialize(dbDocument.JsonString, item.GetType(), Item.JsonOptions);
        }

        public async Task<Item> LoadItemAsync(Item item, ClaimsPrincipal user)
        {
            Item readItem = await LoadItemAsync(item);
            if ((_userSecretsContext is not null) && (readItem.UserSecretKeys is not null))
            {
                string userId = new UserInformation(user).Id;
                foreach (string key in readItem.UserSecretKeys)
                {
                    string secretValue = await _userSecretsContext.GetUserSecretAsync(userId, key);
                    readItem.AddUserSecret(key, secretValue);
                }                 
            }
            return readItem;
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
                items.Add((Item)JsonSerializer.Deserialize(dbDocument.JsonString, item.GetType(), Item.JsonOptions));
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

        public async Task<bool> WriteDbDocumentAsync(DbDocument dbDocument, ClaimsPrincipal user)
        {
            bool wroteDoc = await _noSqlDbContext.WriteDocumentAsync(dbDocument);
            if ((_userSecretsContext is not null) && (dbDocument.UserSecrets is not null))
            {
                string userId = new UserInformation(user).Id;
                foreach (KeyValuePair<string,string> pair in dbDocument.UserSecrets)
                {
                    await _userSecretsContext.PutUserSecretAsync(userId, pair.Key, pair.Value, new TimeSpan(12,0,0));
                }                
            }            
            return wroteDoc;
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

    }
}
