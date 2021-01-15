using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CartaCore.Integration.HyperThought.Data
{
    public class File
    {
        [JsonPropertyName("content")]
        public FileContent Content { get; set; }
        [JsonPropertyName("triples")]
        public List<Triple> Triples { get; set; }
        [JsonPropertyName("metadata")]
        public List<Metadata> Metadata { get; set; }
        [JsonPropertyName("header")]
        public Header Header { get; set; }
        [JsonPropertyName("permissions")]
        public Permissions Permissions { get; set; }
        [JsonPropertyName("restrictions")]
        public Restrictions Restrictions { get; set; }
    }
}