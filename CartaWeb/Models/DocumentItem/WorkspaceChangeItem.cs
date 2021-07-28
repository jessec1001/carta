using System;
using CartaWeb.Models.Data;

namespace CartaWeb.Models.DocumentItem 
{
    /// <summary>
    /// Used to store changes made in a workspace
    /// </summary>
    public class WorkspaceChangeItem : Item
    {
        private string SortKeyPrefix;

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

            if (item.GetType() == typeof(WorkspaceItem))
            {
                ChangeType = WorkspaceChangeEnumeration.Workspace;
                WorkspaceItem workspaceItem = (WorkspaceItem)item;
                Id = workspaceItem.Id;
                Name = workspaceItem.Name;
            }
            if (item.GetType() == typeof(UserItem))
            {
                ChangeType = WorkspaceChangeEnumeration.User;
                UserItem userItem = (UserItem)item;
                Id = userItem.UserInformation.Id;
                Name = userItem.UserInformation.Name;
            }
            if (item.GetType() == typeof(WorkflowAccessItem))
            {
                ChangeType = WorkspaceChangeEnumeration.Workflow;
                WorkflowAccessItem workflowAccessItem = (WorkflowAccessItem)item;
                Id = workflowAccessItem.Id;
                Name = workflowAccessItem.Name;
            }
            if (item.GetType() == typeof(DatasetItem))
            {
                ChangeType = WorkspaceChangeEnumeration.Dataset;
                DatasetItem datasetItem = (DatasetItem)item;
                Id = datasetItem.Id;
                Name = datasetItem.Name;
            }

            SortKeyPrefix = "CHANGE#" + ChangeType + "#";
        }

        /// <summary>
        /// Constructor, used to create an instance for reading all items stored under the partition key identifier. 
        /// </summary>
        /// <param name="partitionKeyId">The partition key identifier.</param>
        /// <param name="changeType">The type of change (only changes of this type will be read).</param>
        public WorkspaceChangeItem(string partitionKeyId, WorkspaceChangeEnumeration changeType)
        {
            PartitionKeyId = partitionKeyId;
            SortKeyPrefix = "CHANGE#" + changeType + "#";
        }

        /// <summary>
        /// Constructor, used to create an instance for reading all items stored under the partition key identifier. 
        /// </summary>
        /// <param name="partitionKeyId">The partition key identifier.</param>
        public WorkspaceChangeItem(string partitionKeyId)
        {
            PartitionKeyId = partitionKeyId;
            SortKeyPrefix = "CHANGE#";
        }

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
            return SortKeyPrefix;
        }
    }

}
