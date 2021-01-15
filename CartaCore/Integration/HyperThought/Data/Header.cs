using System;
using System.Text.Json.Serialization;

namespace CartaCore.Integration.HyperThought.Data
{
    public class Header
    {
        [JsonPropertyName("canonincal-uri")]
        public string CanonicalUri { get; set; }
        [JsonPropertyName("sys-creation-timestamp")]
        public DateTime CreationTime { get; set; }
        [JsonPropertyName("sys-last-modified")]
        public DateTime LastModifiedTime { get; set; }
        [JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; }
        [JsonPropertyName("modifiedBy")]
        public string LastModifiedBy { get; set; }
        [JsonPropertyName("uri")]
        public string Uri { get; set; }
        [JsonPropertyName("pid")]
        public string ProcessId { get; set; }
    }
}