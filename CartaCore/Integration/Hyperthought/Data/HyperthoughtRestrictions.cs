using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents the restrictions on distribution of HyperThought data.
    /// </summary>
    public class HyperthoughtRestrictions
    {
        /// <summary>
        /// The value of the distribution the data is restricted to.
        /// </summary>
        [JsonPropertyName("distribution")]
        public HyperthoughtDistribution Distribution { get; set; }
        /// <summary>
        /// The value of the export the data is restricted to.
        /// </summary>
        [JsonPropertyName("exportControl")]
        public HyperthoughtExportControl ExportControl { get; set; }
        /// <summary>
        /// The value of the security marking the data is restricted to.
        /// </summary>
        /// <value></value>
        [JsonPropertyName("securityMarking")]
        public string SecurityMarking { get; set; }
    }
}