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
        /// Save the given document string under the given key asynchronously.
        /// If the key does not exist, perform a CREATE, else perform an UPDATE.
        /// </summary>
        /// <param name="partitionKey">The partition key of the of document</param>
        /// <param name="sortKey">The sort key of the of document</param>
        /// <param name="docId">The document ID; null if this is the first time the document is saved</param>
        /// <param name="docString">The document string</param>
        /// <returns>A unique document identifier</returns>
        ITask<string> SaveDocumentStringAsync(string partitionKey, string sortKey, string docId, string docString);

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
        /// <param name="partitionKey">The partition key of the of document</param>
        /// <param name="sortKey">The sort key of the of document</param>
        /// <returns>A list of document string</returns>
        ITask<List<string>> LoadDocumentStringsAsync(string partitionKey, string sortKey);

        /// <summary>
        /// Deletes the document stored under the given key asynchronously.
        /// </summary>
        /// <param name="partitionKey">The partition key of the of document</param>
        /// <param name="sortKey">The sort key of the document</param>
        ITask DeleteDocumentStringAsync(string partitionKey, string sortKey);
    }
}
