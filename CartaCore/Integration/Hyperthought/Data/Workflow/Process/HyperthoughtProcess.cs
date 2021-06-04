using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents a HyperThought process with its content, metadata, and structure.
    /// </summary>
    public class HyperthoughtProcess : HyperthoughtObjectBase
    {
        /// <summary>
        /// A value containing the hierarchy of workflow content.
        /// </summary>
        [JsonPropertyName("content")]
        public HyperthoughtProcessContent Content { get; set; }
    }
}