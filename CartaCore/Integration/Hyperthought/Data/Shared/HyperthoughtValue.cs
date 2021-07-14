using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents a HyperThought singleton object.
    /// </summary>
    public class HyperthoughtValue
    {
        /// <summary>
        /// The value stored in the singleton.
        /// </summary>
        [JsonPropertyName("value")]
        public string Data { get; set; }
    }
}