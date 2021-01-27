using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents information about the type and value of data in a HyperThought object.
    /// </summary>
    public class HyperthoughtMetadata
    {
        /// <summary>
        /// The key name of the data.
        /// </summary>
        [JsonPropertyName("keyName")]
        public string Key { get; set; }
        /// <summary>
        /// The value of the data.
        /// </summary>
        [JsonPropertyName("value")]
        public HyperthoughtMetadataValue Value { get; set; }
        /// <summary>
        /// Notes and units of the data.
        /// </summary>
        /// <value></value>
        [JsonPropertyName("annotation")]
        public string Annotation { get; set; }
    }
}