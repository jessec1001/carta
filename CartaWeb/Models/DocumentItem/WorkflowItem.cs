using System.Text.Json;
using NUlid;
using CartaCore.Workflow;
using CartaWeb.Models.Data;
using CartaCore.Persistence;

namespace CartaWeb.Models.DocumentItem
{
    /// <summary>
    /// Used to store workflow information
    /// </summary>
    public class WorkflowItem : Item
    {
        /// <summary>
        /// Flag used to indicate whether the workflow item should be read/persisted as a temporary workflow under a
        /// user or as an officially versioned workflow.
        /// </summary>
        public bool IsTempWorkflow { get; set; } 
        /// <summary>
        /// The workflow
        /// </summary>
        public Workflow Workflow { get; set; }
        /// <summary>
        /// Version information of the workflow
        /// </summary>
        public VersionInformation VersionInformation { get; set; }

        /// <summary>
        /// Parameterless constructor required for deserialization
        /// </summary>
        public WorkflowItem() { }

        /// <summary>
        /// Creates a new instance of the <see cref="WorkflowItem"/> class, used to persist information.
        /// </summary>
        /// <param name="isTempWorkflow">Set to true to read a temporary user workflow, else false.</param>
        /// <param name="partitionKeyId">The partition key identifier (the user identifier if a temporary workflow, else 
        /// the workflow identifier).</param>
        /// <param name="workflow">The workflow.</param>
        /// <param name="versionInformation">Version information</param>
        public WorkflowItem(
            bool isTempWorkflow,
            string partitionKeyId,
            Workflow workflow,
            VersionInformation versionInformation)
        {
            IsTempWorkflow = isTempWorkflow;
            PartitionKeyId = partitionKeyId;
            Id = workflow.Id;
            Workflow = workflow;
            VersionInformation = versionInformation;
            workflow.VersionNumber = VersionInformation.Number;
        }

        /// <summary>
        /// Constructor, used to create an instance for reading all items stored under the partition key identifier. 
        /// </summary>
        /// <param name="isTempWorkflow">Set to true to read a temporary user workflow, else false.</param>
        /// <param name="partitionKeyId">The partition key identifier (the user identifier if temporary, else the
        /// workflow identifier).</param>
        public WorkflowItem(bool isTempWorkflow, string partitionKeyId)
        {
            IsTempWorkflow = isTempWorkflow;
            PartitionKeyId = partitionKeyId;
        }

        /// <summary>
        /// Constructor, used to create an instance for reading a temporary workflow item stored under the
        /// partition and sort key identifiers. 
        /// </summary>
        /// <param name="partitionKeyId">The partition key identifier = user identifier.</param>
        /// <param name="id">The workflow identifier.</param>
        public WorkflowItem(string partitionKeyId, string id)
        {
            IsTempWorkflow = true;
            PartitionKeyId = partitionKeyId;
            Id = id;
        }

        /// <summary>
        /// Constructor, used to create an instance for reading an official workflow item version
        /// </summary>
        /// <param name="partitionKeyId">The partition key identifier = workflow identifier.</param>
        /// <param name="versionNumber">The  workflow version number.</param>
        public WorkflowItem(string partitionKeyId, int versionNumber)
        {
            IsTempWorkflow = false;
            PartitionKeyId = partitionKeyId;
            VersionInformation = new VersionInformation();
            VersionInformation.Number = versionNumber;
        }

        /// <summary>
        /// Codifies the partition key prefix to use for the document.
        /// </summary>
        /// <returns>The partition key prefix.</returns>
        public override string GetPartitionKeyPrefix()
        {
            if (IsTempWorkflow) return "USER#";
            else return "WORKFLOW#";
        }

        /// <summary>
        /// Codifies the sort key prefix to use for the document.
        /// </summary>
        /// <returns>The sort key prefix.</returns>
        public override string GetSortKeyPrefix()
        {
            if (IsTempWorkflow) return "WORKFLOW#";
            else return "VERSION#";
        }

        /// <summary>
        /// Returns the sort key of the document.
        /// </summary>
        /// <returns>The sort key.</returns>
        public override string GetSortKey()
        {
            if (IsTempWorkflow) return GetSortKeyPrefix() + Id;
            else return GetSortKeyPrefix() + VersionInformation.Number;
        }

        /// <summary>
        /// Creates a database document to persist a new item to the database.
        /// </summary>
        /// <returns>A database document.</returns>
        public override DbDocument CreateDbDocument()
        {
            string docId = Ulid.NewUlid().ToString();
            string sortKey = GetSortKeyPrefix() + docId;
            Id = docId;
            Workflow.Id = Id;
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
        /// Creates a database document to save an item to the database.
        /// If the item does not exist, the item will be created, else the item will be updated.
        /// </summary>
        /// <returns>A database document.</returns>
        public override DbDocument SaveDbDocument()
        {
            Workflow.VersionNumber = VersionInformation.Number;
            DbDocument dbDocument = new DbDocument
            (
                GetPartitionKey(),
                GetSortKey(),
                JsonSerializer.Serialize(this, GetType(), JsonOptions),
                DbOperationEnumeration.Save
            );
            return dbDocument;
        }
    }
}