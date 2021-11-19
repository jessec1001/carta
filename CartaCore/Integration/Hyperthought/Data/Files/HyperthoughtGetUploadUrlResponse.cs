using System;
using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought
{
    /// <summary>
    /// Represents a request for a file upload URL from Hyperthought
    /// </summary>
    public class HyperthoughtGetUploadUrlResponse
    {

        /// <summary>
        /// The space UUID
        /// </summary>
        [JsonPropertyName("fileId")]
        public Guid FileId { get; set; }
        /// <summary>
        /// The relative path URL for uploading the file
        /// </summary>
        /// <value></value>
        [JsonPropertyName("url")]
        public string Uri { get; set; }
    }
}

