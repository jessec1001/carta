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
        /// Reads the stored document for the given keys asynchronously.
        /// </summary>
        /// <param name="partitionKey">The partition key of the document</param>
        /// <param name="sortKey">The sort key of the document</param>
        /// <returns>A database document object</returns>
        ITask<DbDocument> ReadDocumentAsync(string partitionKey, string sortKey);

        /// <summary>
        /// Reads all of the documents for the given keys asynchronously.
        /// </summary>
        /// <param name="partitionKey">The partition key of the documents</param>
        /// <param name="sortKeyPrefix">The sort key prefix of the documents</param>
        /// <returns>A list of database document objects</returns>
        ITask<IEnumerable<DbDocument>> ReadDocumentsAsync(string partitionKey, string sortKeyPrefix);

        /// <summary>
        /// Performs a database write operation.
        /// </summary>
        /// <param name="dbDocument">A database document object, containing the partition and sort key, JSON
        /// document string, and type of write operation (Create, Save, Update or Delete) to perform.</param>
        /// <returns>true if the write operation completed successfully, else false</returns>
        ITask<bool> WriteDocumentAsync(DbDocument dbDocument);

        /// <summary>
        /// Performs a list of database write operations within a single database transaction.
        /// </summary>
        /// <param name="dbDocuments">A list of database document objects, with each object containing a partition and
        /// sort key, JSON document string, and type of write operation (Create, Save, Update or Delete) to
        /// perform.</param>
        /// <returns>true if the write operations all completed successfully, else false.</returns>
        ITask<bool> WriteDocumentsAsync(IEnumerable<DbDocument> dbDocuments);

    }
}
