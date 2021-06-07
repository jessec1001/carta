using CartaWeb.Models.Data;

namespace CartaWeb.Models.DocumentItem
{
    /// <summary>
    /// Used to store information such as item name, Id, dates of creation/modification
    /// </summary>
    public class WorkspaceItem
    {
        /// <summary>
        /// The unique worskpace Id, generated when persisting the workspace
        /// </summary>
        public string Id { get; set; }
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
        /// Creates a new instance of the <see cref="WorkspaceItem"/> class.
        /// </summary>
        public WorkspaceItem(string name, UserInformation createdBy)
        {
            Name = name;
            Archived = false;
            DocumentHistory = new DocumentHistory(createdBy);
        }
    }
}
