using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents a HyperThought file with its content, metadata, and structure.
    /// </summary>
    public class HyperthoughtFile
    {
        /// <summary>
        /// A value containing the file content.
        /// </summary>
        [JsonPropertyName("content")]
        public HyperthoughtFileContent Content { get; set; }
        /// <summary>
        /// A value containing all of the file data triples.
        /// </summary>
        [JsonPropertyName("triples")]
        public List<HyperthoughtTriple> Triples { get; set; }
        /// <summary>
        /// A value containing metadata about the types of data in the file.
        /// </summary>
        [JsonPropertyName("metadata")]
        public List<HyperthoughtMetadata> Metadata { get; set; }
        /// <summary>
        /// A value containing information about the file creation and modification.
        /// </summary>
        /// <value></value>
        [JsonPropertyName("header")]
        public HyperthoughtHeader Header { get; set; }
        /// <summary>
        /// A value containing information about which users and groups have permissions to access this file.
        /// </summary>
        /// <value></value>
        [JsonPropertyName("permissions")]
        public HyperthoughtPermissions Permissions { get; set; }
        /// <summary>
        /// A value containing information about where this file is restricted to.
        /// </summary>
        /// <value></value>
        [JsonPropertyName("restrictions")]
        public HyperthoughtRestrictions Restrictions { get; set; }
    }
}