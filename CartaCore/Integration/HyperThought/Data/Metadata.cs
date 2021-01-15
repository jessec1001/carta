using System.Text.Json.Serialization;

namespace CartaCore.Integration.HyperThought.Data
{
    public class Metadata
    {
        [JsonPropertyName("keyName")]
        public string Key { get; set; }
        [JsonPropertyName("value")]
        public MetadataValue Value { get; set; }
        [JsonPropertyName("annotation")]
        public string Annotation { get; set; }
    }
}