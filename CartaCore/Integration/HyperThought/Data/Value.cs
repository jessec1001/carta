using System.Text.Json.Serialization;

namespace CartaCore.Integration.HyperThought.Data
{
    public class Value
    {
        [JsonPropertyName("value")]
        public string Data { get; set; }
    }
}