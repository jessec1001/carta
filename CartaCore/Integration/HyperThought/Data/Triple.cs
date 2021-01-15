using System.Text.Json.Serialization;

namespace CartaCore.Integration.HyperThought.Data
{
    public class Triple
    {
        [JsonPropertyName("triple")]
        public TripleContent Content { get; set; }
    }
}