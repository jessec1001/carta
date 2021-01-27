using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents a HyperThought workflow with its content, metadata, and structure.
    /// </summary>
    public class HyperthoughtWorkflow
    {
        /// <summary>
        /// A value containing the hierarchy of workflow content.
        /// </summary>
        [JsonPropertyName("content")]
        public HyperthoughtWorkflowContent Content { get; set; }
        /// <summary>
        /// A value containing all of the workflow data triples.
        /// </summary>
        [JsonPropertyName("triples")]
        public List<HyperthoughtTriple> Triples { get; set; }
        /// <summary>
        /// A value containing metadata about the types of data in the workflow.
        /// </summary>
        [JsonPropertyName("metadata")]
        public List<HyperthoughtMetadata> Metadata { get; set; }
        /// <summary>
        /// A value containing information about the workflow creation and modification.
        /// </summary>
        [JsonPropertyName("header")]
        public HyperthoughtHeader Header { get; set; }
        /// <summary>
        /// A value containing information about which users and groups have permission to access this workflow.
        /// </summary>
        [JsonPropertyName("permissions")]
        public HyperthoughtPermissions Permissions { get; set; }
        /// <summary>
        /// A value containing information about where this workflow is restricted to.
        /// </summary>
        [JsonPropertyName("restrictions")]
        public HyperthoughtRestrictions Restrictions { get; set; }
    }
}