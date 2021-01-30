using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents a HyperThought project with its content, metadata, and structure.
    /// </summary>
    public class HyperthoughtProject : HyperthoughtObjectBase
    {
        /// <summary>
        /// A value containing the content of this project.
        /// </summary>
        [JsonPropertyName("content")]
        public HyperthoughtProjectContent Content { get; set; }
    }
}