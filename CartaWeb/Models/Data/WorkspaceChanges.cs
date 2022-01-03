using System;
namespace CartaWeb.Models.Data
{
    /// <summary>
    /// Encapsulates changes made in a workspace
    /// </summary>
    public class WorkspaceChanges
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
    }
}
