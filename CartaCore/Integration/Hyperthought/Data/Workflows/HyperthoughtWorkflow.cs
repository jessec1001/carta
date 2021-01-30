using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents a HyperThought workflow with its content, metadata, and structure.
    /// </summary>
    public class HyperthoughtWorkflow : HyperthoughtObjectBase
    {
        /// <summary>
        /// A value containing the hierarchy of workflow content.
        /// </summary>
        [JsonPropertyName("content")]
        public HyperthoughtWorkflowContent Content { get; set; }
    }
}