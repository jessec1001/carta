using System;
using CartaWeb.Models.Data;

namespace CartaWeb.Models.DocumentItem
{
    /// <summary>
    /// Used to store information on workflows that are accessible to a user or workspace
    /// </summary>
    public class WorkflowAccessItem : Item
    {
        /// <summary>
        /// Flag used to indicate whether the workflow access should be read/persisted under a user or workspace.
        /// </summary>
        [NonSerialized()]
        private bool _isUserWorkflow;
        /// <summary>
        /// The workflow name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The workflow version
        /// </summary>
        public VersionInformation VersionInformation { get; set; }
        /// <summary>
        /// Flag indicating whether a workflow has been archived from the view
        /// </summary>
        public bool Archived { get; set; }
        /// <summary>
        /// The workflow access history
        /// </summary>
        public DocumentHistory DocumentHistory { get; set; }

        /// <summary>
        /// Parameterless constructor required for deserialization
        /// </summary>
        public WorkflowAccessItem() { }

        /// <summary>
        /// Creates a new instance of the <see cref="WorkflowAccessItem"/> class, used to persist a worflow access
        /// item.
        /// </summary>
        /// <param name="isUserWorkflow">Set to true to persist workflow access under a user, false to persist
        /// access under a workspace.</param>
        /// <param name="partitionKeyId">The partition key identifier = user or workspace identifier.</param>
        /// <param name="id">The workflow identifier.</param>
        /// <param name="name">The name of the workflow.</param>
        /// <param name="versionInformation">Worfkflow version information.</param>
        public WorkflowAccessItem(
            bool isUserWorkflow,
            string partitionKeyId,
            string id,
            string name,
            VersionInformation versionInformation)
        {
            _isUserWorkflow = isUserWorkflow;
            PartitionKeyId = partitionKeyId;
            Id = id;
            Name = name;
            VersionInformation = versionInformation;
            Archived = false;
        }

        /// <summary>
        /// Constructor, used to create an instance for reading workflow access items stored under a
        /// partition key identifier. 
        /// </summary>
        /// <param name="isUserWorkflow">Set to true to read workflow access stored under a user, false to read
        /// workflow access under a workspace.</param>
        /// <param name="partitionKeyId">The partition key identifier = user or workspace identifier.</param>
        public WorkflowAccessItem(bool isUserWorkflow, string partitionKeyId)
        {
            _isUserWorkflow = isUserWorkflow;
            PartitionKeyId = partitionKeyId;
        }

        /// <summary>
        /// Constructor, used to create an instance for reading a workflow access item stored under the
        /// partition and sort key identifiers. 
        /// </summary>
        /// <param name="isUserWorkflow">Set to true to read workflow access stored under a user, false to read
        /// workflow access under a workspace.</param>
        /// <param name="partitionKeyId">The partition key identifier = user or workspace identifier.</param>
        /// <param name="id">The workflow identifier.</param>
        public WorkflowAccessItem(bool isUserWorkflow, string partitionKeyId, string id)
        {
            _isUserWorkflow = isUserWorkflow;
            PartitionKeyId = partitionKeyId;
            Id = id;
        }

        /// <summary>
        /// Codifies the partition key prefix to use for the document.
        /// </summary>
        /// <returns>The partition key prefix.</returns>
        public override string GetPartitionKeyPrefix()
        {
            if (_isUserWorkflow) return "USER#";
            else return "WORKSPACE#";
        }

        /// <summary>
        /// Codifies the sort key prefix to use for the document.
        /// </summary>
        /// <returns>The sort key prefix.</returns>
        public override string GetSortKeyPrefix()
        {
            return "WORKFLOWACCESS#";
        }
    }
}
