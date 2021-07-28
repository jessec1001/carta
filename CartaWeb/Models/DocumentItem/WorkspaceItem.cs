using System;
using CartaWeb.Models.Data;

namespace CartaWeb.Models.DocumentItem
{
    /// <summary>
    /// Used to store information such as item name, Id, dates of creation/modification
    /// </summary>
    public class WorkspaceItem : Item
    {

        /// <summary>
        /// The workspace name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Flag to indicate if the workspace has been archived
        /// </summary>
        public bool Archived { get; set; }
        /// <summary>
        /// History of the workspace item
        /// </summary>
        public DocumentHistory DocumentHistory { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="WorkspaceItem"/> class.
        /// </summary>
        public WorkspaceItem() {}

        /// <summary>
        /// Creates a new instance of the <see cref="WorkspaceItem"/> class, used to persist a new workspace.
        /// </summary>
        /// <param name="partitionKeyId">The partition key identifier = user identifier.</param>
        /// <param name="name">The workspace name.</param>
        /// <param name="createdBy">Information on the user that created the workspace.</param>
        public WorkspaceItem(string partitionKeyId, string name, UserInformation createdBy)
        {
            PartitionKeyId = partitionKeyId;
            Name = name;
            Archived = false;
            DocumentHistory = new DocumentHistory(createdBy);
        }

        /// <summary>
        /// Constructor, used to create an instance for reading all items stored under the partition key identifier. 
        /// </summary>
        /// <param name="partitionKeyId">The partition key identifier.</param>
        public WorkspaceItem(string partitionKeyId) : base(partitionKeyId) { }

        /// <summary>
        /// Constructor, used to create an instance for reading the item stored under the partition and sort key
        /// identifiers. 
        /// </summary>
        /// <param name="partitionKeyId">The partition key identifier.</param>
        /// <param name="id">The document = sort key identifier.</param>
        public WorkspaceItem(string partitionKeyId, string id) : base(partitionKeyId, id) { }

        /// <summary>
        /// Codifies the partition key prefix to use for the document.
        /// </summary>
        /// <returns>The partition key prefix.</returns>
        public override string GetPartitionKeyPrefix()
        {
            return "USER#";
        }

        /// <summary>
        /// Codifies the sort key prefix to use for the document.
        /// </summary>
        /// <returns>The sort key prefix.</returns>
        public override string GetSortKeyPrefix()
        {
            return "WORKSPACE#";
        }
    }
}
