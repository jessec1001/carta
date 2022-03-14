using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Reflection;
using System.Linq;
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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="noSqlDbContext">The NoSQL DB context used to read and write to the database.</param>
        /// <param name="userSecretsContext">The user secrets context used to read and write secrets.</param>
        public Persistence(INoSqlDbContext noSqlDbContext, IUserSecretsContext userSecretsContext)
        {
            _noSqlDbContext = noSqlDbContext;
            _userSecretsContext = userSecretsContext;
        }

        /// <summary>
        /// Updates an item's secret values from the secrets store
        /// </summary>
        /// <param name="item">The document item to update.</param>
        /// <param name="user">The user claims needed for reading secrets.</param>
        /// <returns>The document item with secrets updated.</returns>
        private async Task<Item> LoadItemSecretsAsync(Item item, ClaimsPrincipal user)
        {
            if (_userSecretsContext is not null)
            {
                string userId = new UserInformation(user).Id;

                List<PropertyInfo> propsInfo =
                    item.GetType().GetProperties().ToList().FindAll(
                        t => t.PropertyType == typeof(UserSecretKeyValuePair));
                foreach (PropertyInfo propInfo in propsInfo)
                {
                    UserSecretKeyValuePair readKeyValuePair = (UserSecretKeyValuePair)propInfo.GetValue(item);
                    string secretValue = await _userSecretsContext.GetUserSecretAsync(userId, readKeyValuePair.Key);
                    UserSecretKeyValuePair setKeyValuePair = new UserSecretKeyValuePair
                    {
                        Key = readKeyValuePair.Key,
                        Value = secretValue
                    };
                    propInfo.SetValue(item, setKeyValuePair);
                }
            }
            return item;
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

        /// <summary>
        /// Reads a database item from the database.
        /// </summary>
        /// <param name="item">The document item, with partition and sort key identifiers set.</param>
        /// <param name="user">The user claims needed for reading secrets.</param>
        /// <returns>The document item read from the database.</returns>
        public async Task<Item> LoadItemAsync(Item item, ClaimsPrincipal user)
        {
            Item readItem = await LoadItemAsync(item);
            readItem = await LoadItemSecretsAsync(readItem, user);
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
                    (Item)JsonSerializer.Deserialize(dbDocument.JsonString, item.GetType(), Item.JsonOptions);
                readItem = await LoadItemSecretsAsync(readItem, user);
                items.Add(readItem);
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
            bool wroteDoc = await _noSqlDbContext.WriteDocumentAsync(dbDocument);
            if ((_userSecretsContext is not null) && (dbDocument.UserSecrets is not null))
            {
                string userId = new UserInformation(user).Id;
                foreach (UserSecretKeyValuePair pair in dbDocument.UserSecrets)
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

        /// <summary>
        /// Writes a list of database documents to the database.
        /// </summary>
        /// <param name="dbDocuments">The database documents.</param>
        /// <param name="user">The user claims needed for persisting secrets.</param>
        /// <returns>true if the write operations all completed successfully, else false.</returns>
        public async Task<bool> WriteDbDocumentsAsync(IEnumerable<DbDocument> dbDocuments, ClaimsPrincipal user)
        {
            bool wroteDocs = await _noSqlDbContext.WriteDocumentsAsync(dbDocuments);
            if (_userSecretsContext is not null)
            {
                string userId = new UserInformation(user).Id;
                foreach (DbDocument dbDocument in dbDocuments)
                {
                    if (dbDocument.UserSecrets is not null)
                    {
                        foreach (UserSecretKeyValuePair pair in dbDocument.UserSecrets)
                        {
                            await _userSecretsContext.PutUserSecretAsync(
                                userId, pair.Key, pair.Value, new TimeSpan(12, 0, 0));
                        }
                    }                  
                }
            }
            return wroteDocs;
        }
    }
}
