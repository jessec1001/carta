using System;
using System.Text.Json.Serialization;
using CartaCore.Serialization.Json;

namespace CartaCore.Integration.Hyperthought
{
    /// <summary>
    /// Represents a request for a file upload URL from Hyperthought
    /// </summary>
    public class HyperthoughtGetUploadUrlRequest
    {
        /// <summary>
        /// The workspace UUID
        /// </summary>
        [JsonPropertyName("workspaceId")]
        public Guid WorkspaceId { get; set; }
        /// <summary>
        /// The path of the file's parent directory by UUID comma-separated list.
        /// </summary>
        /// <value></value>
        [JsonPropertyName("path")]
        public string UUIDPath { get; set; }
        /// <summary>
        /// The file name
        /// </summary>
        [JsonPropertyName("name")]
        public string FileName { get; set; }
        /// <summary>
        /// The file size in bytes
        /// </summary>
        [JsonPropertyName("size")]
        public long Size { get; set; }
    }
}

