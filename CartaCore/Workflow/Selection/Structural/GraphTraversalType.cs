using System.Text.Json.Serialization;

namespace CartaCore.Workflow.Selection
{
    /// <summary>
    /// The type of graph tree traversal we can perform over a set of descendant vertices.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum GraphTraversalType
    {
        /// <summary>
        /// The tree is traversed in preorder fashion.
        /// </summary>
        Preorder,
        /// <summary>
        /// The tree is traversed in postorder fashion.
        /// </summary>
        Postorder
    }
}