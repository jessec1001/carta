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
        public Dictionary<string, string> Users { get; set; }
    }
}