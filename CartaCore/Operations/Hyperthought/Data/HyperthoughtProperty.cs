using CartaCore.Serialization.Json;
using System.Text.Json.Serialization;

namespace CartaCore.Operations.Hyperthought.Data
{
    /// <summary>
    /// A simple structure that represents a Hyperthought property.
    /// </summary>
    public class HyperthoughtProperty
    {
        /// <summary>
        /// The key (name) of the Hyperthought property.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The value of the Hyperthought property.
        /// </summary>
        [JsonConverter(typeof(JsonPrimitiveConverter))]
        public object Value { get; set; }

        /// <summary>
        /// The unit of the Hyperthought property.
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Annotation associated with the Hyperthought property.
        /// </summary>
        public string Annotation { get; set; }
    }
}
