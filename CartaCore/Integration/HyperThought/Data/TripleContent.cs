using System.Text.Json.Serialization;

namespace CartaCore.Integration.HyperThought.Data
{
    public class TripleContent
    {
        [JsonPropertyName("subject")]
        public Value Subject { get; set; }
        [JsonPropertyName("predicate")]
        public Value Predicate { get; set; }
        [JsonPropertyName("object")]
        public Value Object { get; set; }
    }
}