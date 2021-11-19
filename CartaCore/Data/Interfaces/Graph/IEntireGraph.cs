using System.Collections.Generic;
using System.Linq;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a graph where the entirety of the vertices can be enumerated over.
    /// </summary>
    public interface IEntireGraph : IGraph
    {
        /// <summary>
        /// Gets the enumeration of all vertices in the graph.
        /// </summary>
        /// <value>All graph vertices.</value>
        IAsyncEnumerable<IVertex> GetVertices();
        /// <summary>
        /// Gets the enumeration of all edges in the graph.
        /// </summary>
        /// <value>All graph edges.</value>
        IAsyncEnumerable<Edge> GetEdges()
        {
            return GetVertices()
                .SelectMany(vertex => vertex.Edges.ToAsyncEnumerable())
                .Distinct();
        }
    }
}