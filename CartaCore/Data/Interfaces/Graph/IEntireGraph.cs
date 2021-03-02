using System.Collections.Generic;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a graph where the entirety of the vertices and edges can be enumerated over.
    /// </summary>
    public interface IEntireGraph : IGraph
    {
        /// <summary>
        /// Gets the enumeration of all vertices in the graph.
        /// </summary>
        /// <value>All graph vertices.</value>
        IAsyncEnumerable<IVertex> Vertices { get; }
        /// <summary>
        /// Gets the enumeration of all edges in the graph.
        /// </summary>
        /// <value>All graph edges.</value>
        IAsyncEnumerable<Edge> Edges { get; }
    }
}