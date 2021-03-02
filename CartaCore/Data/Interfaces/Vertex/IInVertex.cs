using System.Collections.Generic;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents the base functionality of a vertex with available in-edges.
    /// </summary>
    public interface IInVertex : IVertex
    {
        /// <summary>
        /// Gets the in-edges pointing to this vertex.
        /// </summary>
        /// <value>The enumerable of edges with an <see cref="Edge.Target"/> equal to this vertex's identifier.</value>
        IEnumerable<Edge> InEdges { get; }
    }
}