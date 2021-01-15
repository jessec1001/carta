using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CartaCore.Integration.HyperThought.Data
{
    public class Permissions
    {
        [JsonPropertyName("projects")]
        public Dictionary<string, string> Projects { get; set; }
        [JsonPropertyName("groups")]
        public Dictionary<string, string> Groups { get; set; }
        [JsonPropertyName("users")]
        public Dictionary<string, string> Users { get; set; }
    }
}