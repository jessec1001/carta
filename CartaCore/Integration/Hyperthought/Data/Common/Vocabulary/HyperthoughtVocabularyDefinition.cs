using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents a simple HyperThought vocabulary definition.
    /// </summary>
    public class HyperthoughtVocabularyDefinition
    {
        /// <summary>
        /// The topic of the definition. For instance, "Materials Property".
        /// </summary>
        [JsonPropertyName("topic")]
        public string Topic { get; set; }
        /// <summary>
        /// The unique key of this definition.
        /// </summary>
        [JsonPropertyName("key")]
        public string Key { get; set; }
        /// <summary>
        /// The definition of the vocabulary term including its origin and licensing information.
        /// </summary>
        [JsonPropertyName("definition")]
        public string Definition { get; set; }
    }
}