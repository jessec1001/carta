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
        /// The projects that have access to this data.
        /// </summary>
        [JsonPropertyName("projects")]
        public Dictionary<string, string> Projects { get; set; }
        /// <summary>
        /// The groups that have access to this data.
        /// </summary>
        [JsonPropertyName("groups")]
        public Dictionary<string, string> Groups { get; set; }
        /// <summary>
        /// The users that have access to this data.
        /// </summary>
        [JsonPropertyName("users")]
        public Dictionary<string, HyperthoughtUserPermissions> Users { get; set; }

        /// <summary>
        /// Creates a permissions object to allow a process to be editted by a particular project. 
        /// </summary>
        /// <param name="projectId">The unique ID of the project to allow modifications from.</param>
        /// <returns>The constructed permissions.</returns>
        public static HyperthoughtPermissions FromProject(Guid projectId)
        {
            return new HyperthoughtPermissions
            {
                Users = new Dictionary<string, HyperthoughtUserPermissions>(),
                Groups = new Dictionary<string, string>(),
                Projects = new Dictionary<string, string>()
                {
                    [projectId.ToString()] = "edit"
                }
            };
        }
        /// <summary>
        /// Creates a permissions object to allow a process to be editted by a particular project.
        /// </summary>
        /// <param name="project">The project to allow modifications from.</param>
        /// <returns>The constructed permissions.</returns>
        public static HyperthoughtPermissions FromProject(HyperthoughtProject project)
        {
            return HyperthoughtPermissions.FromProject(project.Content.PrimaryKey);
        }
    }
}