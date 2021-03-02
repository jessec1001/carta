using System.Collections.Generic;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents the base functionality of a vertex with available out-edges.
    /// </summary>
    public interface IOutVertex : IVertex
    {
        /// <summary>
        /// Gets the out-edges pointing to this vertex.
        /// </summary>
        /// <value>The enumerable of edges with an <see cref="Edge.Source"/> equal to this vertex's identifier.</value>
        IEnumerable<Edge> OutEdges { get; }
    }
}