using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents a container for a HyperThought triple statement.
    /// </summary>
    public class HyperthoughtTriple
    {
        /// <summary>
        /// A value with the content of the statement.
        /// </summary>
        [JsonPropertyName("triple")]
        public HyperthoughtTripleContent Content { get; set; }
    }
}