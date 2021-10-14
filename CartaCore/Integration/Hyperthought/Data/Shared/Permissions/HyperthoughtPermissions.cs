using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents the access permissions of a HyperThought data to specific projects, groups, and users. 
    /// </summary>
    public class HyperthoughtPermissions
    {
        /// <summary>
        /// The workspaces that have access to this data.
        /// </summary>
        [JsonPropertyName("workspaces")]
        public Dictionary<string, string> Workspaces { get; set; }
        /// <summary>
        /// The users that have access to this data.
        /// </summary>
        [JsonPropertyName("users")]
        public Dictionary<string, HyperthoughtUserPermissions> Users { get; set; }

        /// <summary>
        /// Creates a permissions object to allow a process to be editted by a particular workspace. 
        /// </summary>
        /// <param name="workspaceId">The unique ID of the workspace to allow modifications from.</param>
        /// <returns>The constructed permissions.</returns>
        public static HyperthoughtPermissions FromWorkspace(Guid workspaceId)
        {
            return new HyperthoughtPermissions
            {
                Users = new Dictionary<string, HyperthoughtUserPermissions>(),
                Workspaces = new Dictionary<string, string>()
                {
                    [workspaceId.ToString()] = "edit"
                }
            };
        }
        /// <summary>
        /// Creates a permissions object to allow a process to be editted by a particular workspace.
        /// </summary>
        /// <param name="workspace">The workspace to allow modifications from.</param>
        /// <returns>The constructed permissions.</returns>
        public static HyperthoughtPermissions FromWorkspace(HyperthoughtWorkspace workspace)
        {
            return HyperthoughtPermissions.FromWorkspace(workspace.PrimaryKey);
        }
    }
}