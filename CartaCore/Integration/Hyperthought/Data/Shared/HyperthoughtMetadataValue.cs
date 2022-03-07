using System.Text.Json.Serialization;
using CartaCore.Serialization.Json;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents a metadata value of HyperThought data.
    /// </summary>
    public class HyperthoughtMetadataValue
    {
        /// <summary>
        /// The type of data.
        /// </summary>
        [JsonPropertyName("type")]
        public HyperthoughtDataType Type { get; set; }
        /// <summary>
        /// A reference to another object.
        /// </summary>
        [JsonPropertyName("link")]
        [JsonConverter(typeof(JsonObjectConverter))]
        public object Link { get; set; }
        /// <summary>
        /// The display text - only applicable to files, and is the human readable full file path
        /// </summary>
        [JsonPropertyName("displayText")]
        public string DisplayText { get; set; }
    }
}