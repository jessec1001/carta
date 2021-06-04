using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents a HyperThought group with its content, metadata, and structure.
    /// </summary>
    public class HyperthoughtGroup : HyperthoughtObjectBase
    {
        /// <summary>
        /// A value containing the group contents.
        /// </summary>
        [JsonPropertyName("content")]
        public HyperthoughtGroupContent Content { get; set; }
    }
}