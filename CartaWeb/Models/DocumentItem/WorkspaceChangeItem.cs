using System;
using CartaWeb.Models.Data;

namespace CartaWeb.Models.DocumentItem 
{
    /// <summary>
    /// Used to store changes made in a workspace
    /// </summary>
    public class WorkspaceChangeItem : Item
    {
        private string _sortKeyPrefix;

        /// <summary>
        /// Describes the type of object that changed
        /// </summary>
        public WorkspaceChangeEnumeration ChangeType { get; set; }

        /// <summary>
        /// The name of the object that changed 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The action that was performed by the change
        /// </summary>
        public WorkspaceAction WorkspaceAction { get; set; }

        /// <summary>
        /// Additional information about the changes 
        /// </summary>
        public WorkspaceChangeInformation WorkspaceChangeInformation { get; set; }

        /// <summary>
        /// Parameterless constructor required for deserialization
        /// </summary>
        public WorkspaceChangeItem() {}

        /// <summary>
        /// Creates a new instance of the <see cref="WorkspaceChangeItem"/> class, used to persist a new workspace
        /// change item.
        /// </summary>
        /// <param name="partitionKeyId">The partition key identifier = workspace identifier.</param>
        /// <param name="userName">The name of the user that performed the change.</param>
        /// <param name="actionType">The action that was performed by the change.</param>
        /// <param name="item">The item that changed.</param>
        public WorkspaceChangeItem(
            string partitionKeyId,
            string userName,
            WorkspaceActionEnumeration actionType,
            Item item)
        {
            PartitionKeyId = partitionKeyId;

            WorkspaceAction = new WorkspaceAction();
            WorkspaceAction.Type = actionType;
            WorkspaceAction.UserName = userName;
            WorkspaceAction.DateTime = DateTime.Now;

            switch (item)
            {
                case WorkspaceItem workspaceItem:
                    ChangeType = WorkspaceChangeEnumeration.Workspace;
                    Id = workspaceItem.Id;
                    Name = workspaceItem.Name;
                    break;
                case UserItem userItem:
                    ChangeType = WorkspaceChangeEnumeration.User;
                    Id = userItem.UserInformation.Id;
                    Name = userItem.UserInformation.Name;
                    break;
                case WorkflowAccessItem workflowAccessItem:
                    ChangeType = WorkspaceChangeEnumeration.Workflow;
                    Id = workflowAccessItem.Id;
                    Name = workflowAccessItem.Name;
                    break;
                case DatasetItem datasetItem:
                    ChangeType = WorkspaceChangeEnumeration.Dataset;
                    Id = datasetItem.Id;
                    Name = datasetItem.Name;
                    break;
            }

            _sortKeyPrefix = "CHANGE#" + ChangeType + "#";
        }

        /// <summary>
        /// Constructor, used to create an instance for reading all items stored under the partition key identifier. 
        /// </summary>
        /// <param name="partitionKeyId">The partition key identifier.</param>
        /// <param name="changeType">The type of change (only changes of this type will be read).</param>
        public WorkspaceChangeItem(string partitionKeyId, WorkspaceChangeEnumeration changeType)
        {
            PartitionKeyId = partitionKeyId;
            _sortKeyPrefix = "CHANGE#" + changeType + "#";
        }

        /// <summary>
        /// Constructor, used to create an instance for reading all items stored under the partition key identifier. 
        /// </summary>
        /// <param name="partitionKeyId">The partition key identifier.</param>
        public WorkspaceChangeItem(string partitionKeyId)
        {
            PartitionKeyId = partitionKeyId;
            _sortKeyPrefix = "CHANGE#";
        }

        /// <summary>
        /// Codifies the partition key prefix to use for the document.
        /// </summary>
        /// <returns>The partition key prefix.</returns>
        public override string PartitionKeyPrefix { get { return "WORKSPACE#"; } }

        /// <summary>
        /// Codifies the sort key prefix to use for the document.
        /// </summary>
        /// <returns>The sort key prefix.</returns>
        public override string SortKeyPrefix { get { return _sortKeyPrefix; } }

    }

}
