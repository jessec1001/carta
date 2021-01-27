using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents a HyperThought semantic triple statement.
    /// </summary>
    public class HyperthoughtTripleContent
    {
        /// <summary>
        /// The subject value of the statement.
        /// </summary>
        [JsonPropertyName("subject")]
        public HyperthoughtValue Subject { get; set; }
        /// <summary>
        /// The predicate value of the statement.
        /// </summary>
        [JsonPropertyName("predicate")]
        public HyperthoughtValue Predicate { get; set; }
        /// <summary>
        /// The object of the statement.
        /// </summary>
        [JsonPropertyName("object")]
        public HyperthoughtValue Object { get; set; }
    }
}