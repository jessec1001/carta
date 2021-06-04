using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents a simple HyperThought unit description.
    /// </summary>
    public class HyperthoughtUnit
    {
        /// <summary>
        /// The type of unit. For instance, "Volume" or "Mass".
        /// </summary>
        [JsonPropertyName("quantityKindLabel")]
        public string Kind { get; set; }
        /// <summary>
        /// The label of the unit. For instance, "meters-per-second" or "kilograms".
        /// </summary>
        [JsonPropertyName("unitLabel")]
        public string Label { get; set; }
    }
}