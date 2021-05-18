using System.Collections.Generic;

using MorseCode.ITask;

namespace CartaCore.Persistence
{
    /// <summary>
    /// Represents a database context for accessing a NoSQL database
    /// </summary>
    public interface INoSqlDbContext
    {
        /// <summary>
        /// Creates the given document string under the given key asynchronously.
        /// If the key does not exist, perform a CREATE, else perform an UPDATE.
        /// </summary>
        /// <param name="partitionKey">The partition key of the of document</param>
        /// <param name="sortKeyPrefix">The sort key prefix of the of document</param>
        /// <param name="docString">The document string</param>
        /// <returns>An unique document identifier; null if the record already exists and the create failed</returns>
        ITask<string> CreateDocumentStringAsync(string partitionKey, string sortKeyPrefix, string docString);

        /// <summary>
        /// Load the stored document for the given keys asynchronously.
        /// </summary>
        /// <param name="partitionKey">The partition key of the of document</param>
        /// <param name="sortKey">The sort key of the of document</param>
        /// <returns>A document string</returns>
        ITask<string> LoadDocumentStringAsync(string partitionKey, string sortKey);

        /// <summary>
        /// Load all of the documents for the given keys asynchronously.
        /// </summary>
        /// <param name="partitionKey">The partition key of the of documents</param>
        /// <param name="sortKeyPrefix">The sort key prefix of the of documents</param>
        /// <returns>A list of document strings</returns>
        ITask<List<string>> LoadDocumentStringsAsync(string partitionKey, string sortKeyPrefix);

        /// <summary>
        /// Updates the given document string under the given key asynchronously.
        /// </summary>
        /// <param name="partitionKey">The partition key of the of document</param>
        /// <param name="sortKey">The sort key of the of document</param>
        /// <param name="docString">The document string</param>
        /// <returns>Returns true if the update was successful, otherwise false</returns>
        ITask<bool> UpdateDocumentStringAsync(string partitionKey, string sortKey, string docString);

        /// <summary>
        /// Creates or updates the given document string under the given key asynchronously.
        /// If a document under the key does not exist, the entry is created, otherwise it is updated. 
        /// </summary>
        /// <param name="partitionKey">The partition key of the of document</param>
        /// <param name="sortKey">The sort key of the of document</param>
        /// <param name="docString">The document string</param>
        ITask SaveDocumentStringAsync(string partitionKey, string sortKey, string docString);

        /// <summary>
        /// Deletes the document stored under the given key asynchronously.
        /// </summary>
        /// <param name="partitionKey">The partition key of the of document</param>
        /// <param name="sortKey">The sort key of the document</param>
        ITask DeleteDocumentStringAsync(string partitionKey, string sortKey);
    }
}
