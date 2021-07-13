using System;
using CartaWeb.Models.Data;

namespace CartaWeb.Models.DocumentItem
{
    /// <summary>
    /// Used to store changes made in a workspace
    /// </summary>
    public class WorkspaceChangeItem
    {
        /// <summary>
        /// Describes the type of object that changed
        /// </summary>
        public WorkspaceChangeEnumeration ChangeType { get; set; }

        /// <summary>
        /// The identifier of the object that changed
        /// </summary>
        public string Id { get; set; }

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
        /// Creates a new instance of the <see cref="WorkspaceChangeItem"/> class.
        /// </summary>
        public WorkspaceChangeItem() { }

        /// <summary>
        /// Creates a new instance of the <see cref="WorkspaceChangeItem"/> class.
        /// </summary>
        public WorkspaceChangeItem(
            WorkspaceChangeEnumeration changeType,
            string id,
            string name,
            WorkspaceActionEnumeration actionType,
            string userName)
        {
            ChangeType = changeType;
            Id = id;
            Name = name;
            WorkspaceAction = new WorkspaceAction();
            WorkspaceAction.Type = actionType;
            WorkspaceAction.UserName = userName;
            WorkspaceAction.DateTime = DateTime.Now;
        }
    }
}
