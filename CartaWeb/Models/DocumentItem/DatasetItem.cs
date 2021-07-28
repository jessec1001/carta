using System;
using CartaWeb.Models.Data;

namespace CartaWeb.Models.DocumentItem
{
    /// <summary>
    /// Represents information about data set storage
    /// </summary>
    public class DatasetItem : Item
    {
        /// <summary>
        /// The data source
        /// </summary>
        public DataSource Source { get; set; }
        /// <summary>
        /// The data resource
        /// </summary>
        public string Resource { get; set; }
        /// <summary>
        /// Optional alias, to support e.g. an original vs. transformed data set under the same workspace
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// History of the data set item
        /// </summary>
        public DocumentHistory DocumentHistory { get; set; }
        /// <summary>
        /// Optional workflow identifier to apply by default to the data set
        /// </summary>
        public string WorkflowId { get; set; }
        /// <summary>
        /// Optional workflow version number
        /// </summary>
        public int? VersionNumber { get; set; }

        /// <summary>
        /// Parameterless constructor required for deserialization
        /// </summary>
        public DatasetItem() { }

        /// <summary>
        /// Creates a new instance of the <see cref="DatasetItem"/> class, used to persist a new data set item.
        /// </summary>
        /// <param name="partitionKeyId">The partition key identifier.</param>
        /// <param name="source">The data source.</param>
        /// <param name="resource">The data resource.</param>
        /// <param name="userInformation">Information on the user that created the dataset item.</param>
        public DatasetItem(string partitionKeyId, DataSource source, string resource, UserInformation userInformation)
        {
            PartitionKeyId = partitionKeyId;
            Source = source;
            Resource = resource;
            DocumentHistory = new DocumentHistory(userInformation);
        }

        /// <summary>
        /// Constructor, used to create an instance for reading all items stored under the partition key identifier. 
        /// </summary>
        /// <param name="partitionKeyId">The partition key identifier.</param>
        public DatasetItem(string partitionKeyId) : base(partitionKeyId) { }

        /// <summary>
        /// Constructor, used to create an instance for reading the item stored under the partition and sort key
        /// identifiers. 
        /// </summary>
        /// <param name="partitionKeyId">The partition key identifier.</param>
        /// <param name="id">The document = sort key identifier.</param>
        public DatasetItem(string partitionKeyId, string id) : base(partitionKeyId, id) { }

        /// <summary>
        /// Codifies the partition key prefix to use for the document.
        /// </summary>
        /// <returns>The partition key prefix.</returns>
        public override string GetPartitionKeyPrefix()
        {
            return "WORKSPACE#";
        }

        /// <summary>
        /// Codifies the sort key prefix to use for the document.
        /// </summary>
        /// <returns>The sort key prefix.</returns>
        public override string GetSortKeyPrefix()
        {
            return "DATASET#";
        }
    }
}
