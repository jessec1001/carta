using System;
namespace CartaWeb.Models.Data
{
    /// <summary>
    /// Encapsulates actions performed in a workspace
    /// </summary>
    public class WorkspaceAction
    {
        /// <summary>
        /// The type of action performed
        /// </summary>
        public WorkspaceActionEnumeration Type { get; set; }

        /// <summary>
        /// The name of the user that performed the action
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The time the action was performed
        /// </summary>
        public DateTime DateTime { get; set; }
    }

}
