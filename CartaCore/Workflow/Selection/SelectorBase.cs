using CartaCore.Data;
using CartaCore.Serialization.Json;

namespace CartaCore.Workflow.Selection
{
    /// <summary>
    /// Represents a selection of vertices and edges in a graph.
    /// </summary>
    [DiscriminantBase]
    public abstract class SelectorBase
    {
        /// <summary>
        /// Determines whether the selector contains the specified vertex.
        /// </summary>
        /// <param name="vertex">The vertex.</param>
        /// <returns><c>true</c> if the selector contains the vertex; otherwise <c>false</c>.</returns>
        public virtual bool ContainsVertex(IVertex vertex) => true;
        public virtual bool ContainsProperty(Property property) => true;
        public virtual bool ContainsValue(object value) => true;
    }
}