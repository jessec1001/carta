using System;
using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents resources used by workspace.
    /// </summary>
    public class HyperthoughtFileResource
    {
        /// <summary>
        /// The API resource ID used to access this workspace.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }
        /// <summary>
        /// A URL from which the file can get downloaded
        /// </summary>
        [JsonPropertyName("generateDownloadUrl")]
        public string DownloadUrl { get; set; }
    }
}