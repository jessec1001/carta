using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CartaCore.Serialization.Json;

namespace CartaCore.Integration.Hyperthought.Data
{

    /// <summary>
    /// Represents a Hyperthought create folder request.
    /// </summary>
    public class HyperthoughtCreateFolderRequest
    {
        /// <summary>
        /// The workspace UUID
        /// </summary>
        [JsonPropertyName("space_id")]
        public Guid WorkspaceId { get; set; }
        /// <summary>
        /// The path of the file's parent directory by UUID comma-separated list.
        /// </summary>
        /// <value></value>
        [JsonPropertyName("path")]
        public string UUIDPath { get; set; }
        /// <summary>
        /// The name of the folder to be created
        /// </summary>
        [JsonPropertyName("name")]
        public string FolderName { get; set; }
        /// <summary>
        /// A value containing metadata about the types of data.
        /// </summary>
        [JsonPropertyName("metadata")]
        public List<HyperthoughtMetadata> Metadata { get; set; }
    }
}
