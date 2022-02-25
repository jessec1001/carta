using System.Collections.Generic;

namespace CartaCore.Graph.Components
{
    /// <summary>
    /// A graph component that allows enumeration over vertices and edges.
    /// </summary>
    /// <typeparam name="TVertex">The type of vertex.</typeparam>
    /// <typeparam name="TEdge">The type of edge.</typeparam>
    public interface IEnumerableComponent<out TVertex, out TEdge>
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
        async IAsyncEnumerable<TEdge> GetEdges()
        {
            await foreach (TVertex vertex in GetVertices())
                foreach (TEdge edge in vertex.OutEdges)
                    yield return edge;
        }
    }
    /// <summary>
    /// A graph component that allows enumeration over vertices and edges.
    /// </summary>
    public interface IEnumerableComponent : IEnumerableComponent<IVertex<IEdge>, IEdge> { }
}