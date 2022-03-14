using System;
using System.Collections.Generic;

namespace CartaCore.Persistence
{
    /// <summary>
    /// Contains properties needed by a <see cref="INoSqlDbContext"/> class to write and read JSON documents to and
    /// from a NoSQL database.
    /// </summary>
    public class DbDocument
    {
        /// <summary>
        /// The partition key under which the document is stored.
        /// </summary>
        public string PartitionKey { get; set; }

        /// <summary>
        /// The sort key under which the document is stored.
        /// </summary>
        public string SortKey { get; set; }

        /// <summary>
        /// A text representation of the JSON document.
        /// </summary>
        public string JsonString { get; set; }

        // <summary>
        /// A list of user secrets that should be stored safely
        /// </summary>
        public List<UserSecretKeyValuePair> UserSecrets;

        /// <summary>
        /// The type of database operation - Create, Read, Update, Save or Delete.
        /// </summary>
        public DbOperationType Operation { get; set; }

        /// <summary>
        /// Constructor for Create, Read, Updated and Save operations.
        /// <param name="partitionKey">The partition key under which the document is stored.</param>
        /// <param name="sortKey">The sort key under which the document is stored.</param>
        /// <param name="jsonString">A text representation of the JSON document.</param>
        /// <param name="operation">The type of database operation - Create, Read, Update, Save.</param>
        /// </summary>
        public DbDocument(string partitionKey, string sortKey, string jsonString, DbOperationType operation)
        {
            if (operation == DbOperationType.Delete)
                throw new NotSupportedException("Delete operations are not supported by this constructor");
            PartitionKey = partitionKey;
            SortKey = sortKey;
            JsonString = jsonString;
            Operation = operation;
        }

        /// <summary>
        /// Constructor for Create, Read, Update and Save operations.
        /// <param name="partitionKey">The partition key under which the document is stored.</param>
        /// <param name="sortKey">The sort key under which the document is stored.</param>
        /// <param name="userSecrets">A list of user secret key value pairs that should be stored safely</param>
        /// <param name="jsonString">A text representation of the JSON document.</param>
        /// <param name="operation">The type of database operation - Create, Read, Update, Save.</param>
        /// </summary>
        public DbDocument(
            string partitionKey,
            string sortKey,
            List<UserSecretKeyValuePair> userSecrets,
            string jsonString,
            DbOperationType operation) : this(partitionKey, sortKey, jsonString, operation)
        {
            UserSecrets = userSecrets;
        }

        /// <summary>
        /// Constructor for Delete operations.
        /// <param name="partitionKey">The partition key under which the document is stored.</param>
        /// <param name="sortKey">The sort key under which the document is stored.</param>
        /// <param name="operation">The type of database operation - must be set to Delete.</param>
        /// </summary>
        public DbDocument(string partitionKey, string sortKey, DbOperationType operation)
        {
            if (operation != DbOperationType.Delete)
                throw new NotSupportedException("Only delete operations are supported by this constructor");
            PartitionKey = partitionKey;
            SortKey = sortKey;
            Operation = operation;
        }
    }
}
