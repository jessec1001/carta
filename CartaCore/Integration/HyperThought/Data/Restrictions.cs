using System.Text.Json.Serialization;

namespace CartaCore.Integration.HyperThought.Data
{
    public class Restrictions
    {
        [JsonPropertyName("distribution")]
        public Distribution Distribution { get; set; }
        [JsonPropertyName("exportControl")]
        public ExportControl ExportControl { get; set; }
        [JsonPropertyName("securityMarking")]
        public string SecurityMarking { get; set; }
    }
}