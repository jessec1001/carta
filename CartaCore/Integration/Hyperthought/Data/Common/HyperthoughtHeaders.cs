using System;
using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents information about the creation, modification, and storage of a HyperThought object.
    /// </summary>
    public class HyperthoughtHeaders
    {
        /// <summary>
        /// Where the object is stored regardless of version.
        /// </summary>
        [JsonPropertyName("canonincal-uri")]
        public string CanonicalUri { get; set; }
        /// <summary>
        /// Where the object is stored including version.
        /// </summary>
        [JsonPropertyName("uri")]
        public string Uri { get; set; }
        /// <summary>
        /// When the resource was created.
        /// </summary>
        [JsonPropertyName("sys-creation-timestamp")]
        public DateTime CreationTime { get; set; }
        /// <summary>
        /// When the resource was last modified.
        /// </summary>
        [JsonPropertyName("sys-last-modified")]
        public DateTime LastModifiedTime { get; set; }
        /// <summary>
        /// What user the resource was created by.
        /// </summary>
        [JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; }
        /// <summary>
        /// What user the resource was last modified by.
        /// </summary>
        /// <value></value>
        [JsonPropertyName("modifiedBy")]
        public string LastModifiedBy { get; set; }
        /// <summary>
        /// The process ID of the resource.
        /// </summary>
        [JsonPropertyName("pid")]
        public Guid ProcessId { get; set; }
    }
}