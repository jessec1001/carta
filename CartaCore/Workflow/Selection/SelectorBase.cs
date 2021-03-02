using CartaCore.Data;

namespace CartaCore.Workflow.Selection
{
    /// <summary>
    /// Represents a selection of vertices and edges in a freeform graph.
    /// </summary>
    public abstract class SelectorBase
    {
        /// <summary>
        /// Gets the type of selector as a string representation.
        /// </summary>
        /// <value>The type of selector represented as text.</value>
        public abstract string Type { get; }

        /// <summary>
        /// Determines whether the selector contains the specified vertex.
        /// </summary>
        /// <param name="vertex">The vertex.</param>
        /// <returns><c>true</c> if the selector contains the vertex; otherwise <c>false</c>.</returns>
        public abstract bool Contains(Vertex vertex);
    }
}