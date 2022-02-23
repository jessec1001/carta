using System.Collections.Generic;
using System.Linq;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a graph where the entirety of the vertices can be enumerated over.
    /// </summary>
    /// <typeparam name="TVertex">The type of vertex.</typeparam>
    /// <typeparam name="TEdge">The type of edge.</typeparam>
    public interface IEntireGraph<out TVertex, out TEdge> : IGraph
        where TVertex : IVertex<TEdge>
        where TEdge : IEdge
    {
        /// <summary>
        /// Gets the enumeration of all vertices in the graph.
        /// </summary>
        /// <value>All graph vertices.</value>
        IAsyncEnumerable<TVertex> GetVertices();
        /// <summary>
        /// Gets the enumeration of all edges in the graph.
        /// </summary>
        /// <value>All graph edges.</value>
        IAsyncEnumerable<TEdge> GetEdges()
        {
            return GetVertices()
                .SelectMany(vertex => vertex.Edges.ToAsyncEnumerable())
                .Distinct();
        }
    }
    /// <summary>
    /// Represents a graph where the entirety of the vertices can be enumerated over.
    /// </summary>
    public interface IEntireGraph : IEntireGraph<IVertex, IEdge> { }
}