using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    // TODO: This is a DTO not a standard data structure.
    /// <summary>
    /// Represents a request for a file upload URL from Hyperthought
    /// </summary>
    public class HyperthoughtGetDownloadUrlResponse
    {

        /// <summary>
        /// The relative path URL for uploading the file
        /// </summary>
        /// <value></value>
        [JsonPropertyName("url")]
        public string Uri { get; set; }
    }
}
