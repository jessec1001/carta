using System.Text.Json.Serialization;

namespace CartaCore.Integration.HyperThought.Data
{
    public class MetadataValue
    {
        [JsonPropertyName("type")]
        public DataType Type { get; set; }
        [JsonPropertyName("link")]
        public string Link { get; set; }
    }
}