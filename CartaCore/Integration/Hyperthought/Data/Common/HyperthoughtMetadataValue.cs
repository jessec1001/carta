using System.Text.Json.Serialization;

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
        public object Link { get; set; }
    }
}