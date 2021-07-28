﻿using System;
using System.Text.Json;
using NUlid;
using CartaCore.Persistence;
using CartaCore.Serialization.Json;

namespace CartaWeb.Models.DocumentItem
{
    /// <summary>
    /// Abstract base class for documents that will be persisted to a NoSQL database store.
    /// Such documents are stored under a partition key and sort key, and child classes should specify
    /// the required prefixes associated with the keys. 
    /// </summary>
    public abstract class Item
    {
        /// <summary>
        /// Options for serialization
        /// </summary>
        public static JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        /// <summary>
        /// Static constructor for initializing JSON serialization/deserialization options
        /// </summary>
        static Item()
        {
            JsonOptions.PropertyNameCaseInsensitive = false;
            JsonOptions.IgnoreNullValues = true;
            JsonOptions.Converters.Insert(0, new JsonDiscriminantConverter());
        }

        /// <summary>
        /// The partition key identifier, sans prefix. 
        /// </summary>
        [NonSerialized()] protected string PartitionKeyId;

        /// <summary>
        /// The unique identifier of the item.
        /// </summary>
        public string Id { get; set; }


        /// <summary>
        /// Parameterless constructor. 
        /// </summary>
        public Item() { }

        /// <summary>
        /// Constructor, used to create an instance for reading all items stored under the partition key identifier. 
        /// </summary>
        /// <param name="partitionKeyId">The partition key identifier.</param>
        public Item(string partitionKeyId)
        {
            PartitionKeyId = partitionKeyId;
        }

        /// <summary>
        /// Constructor, used to create an instance for reading the item stored under the partition and sort key
        /// identifiers. 
        /// </summary>
        /// <param name="partitionKeyId">The partition key identifier.</param>
        /// <param name="id">The document = sort key identifier.</param>
        public Item(string partitionKeyId, string id)
        {
            PartitionKeyId = partitionKeyId;
            Id = id;
        }

        /// <summary>
        /// Sets the partition key identifier 
        /// </summary>
        public void SetPartitionKeyId(string partitionKeyId)
        {
            PartitionKeyId = partitionKeyId;
        }

        /// <summary>
        /// Codifies the partition key prefix to use for the document.
        /// </summary>
        /// <returns>The partition key prefix.</returns>
        public abstract string GetPartitionKeyPrefix();

        /// <summary>
        /// Codifies the sort key prefix to use for the document.
        /// </summary>
        /// <returns>The sort key prefix.</returns>
        public abstract string GetSortKeyPrefix();

        /// <summary>
        /// Returns the partition key of the document.
        /// </summary>
        /// <returns>The partition key.</returns>
        public virtual string GetPartitionKey()
        {
            return GetPartitionKeyPrefix() + PartitionKeyId;
        }

        /// <summary>
        /// Returns the sort key of the document.
        /// </summary>
        /// <returns>The sort key.</returns>
        public virtual string GetSortKey()
        {
            return GetSortKeyPrefix() + Id;
        }

        /// <summary>
        /// Creates a database document to persist a new item to the database.
        /// </summary>
        /// <returns>A database document.</returns>
        public virtual DbDocument CreateDbDocument()
        {
            string docId = Ulid.NewUlid().ToString();
            string sortKey = GetSortKeyPrefix() + docId;
            if (Id is null) Id = docId;
            DbDocument dbDocument = new DbDocument
            (
                GetPartitionKey(),
                sortKey,
                JsonSerializer.Serialize(this, GetType(), JsonOptions),
                DbOperationEnumeration.Create
            );
            return dbDocument;
        }

        /// <summary>
        /// Creates a database document to update an existing item.
        /// </summary>
        /// <returns>A database document.</returns>
        public virtual DbDocument UpdateDbDocument()
        {
            return new DbDocument
            (
                GetPartitionKey(),
                GetSortKey(),
                JsonSerializer.Serialize(this, GetType(), JsonOptions),
                DbOperationEnumeration.Update
            );
        }

        /// <summary>
        /// Creates a database document to save an item to the database.
        /// If the item does not exist, the item will be created, else the item will be updated.
        /// </summary>
        /// <returns>A database document.</returns>
        public virtual DbDocument SaveDbDocument()
        {
            return new DbDocument
            (
                GetPartitionKey(),
                GetSortKey(),
                JsonSerializer.Serialize(this, GetType(), JsonOptions),
                DbOperationEnumeration.Save
            );
        }

        /// <summary>
        /// Creates a database document to delete an item from the database.
        /// </summary>
        /// <returns>A database document.</returns>
        public virtual DbDocument DeleteDbDocument()
        {
            return new DbDocument
            (
                GetPartitionKey(),
                GetSortKey(),
                DbOperationEnumeration.Delete
            );
        }

    }
}