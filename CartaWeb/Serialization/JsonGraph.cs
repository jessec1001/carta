using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Graphs;
using CartaCore.Graphs.Components;

namespace CartaWeb.Serialization
{
    /// <summary>
    /// Represents a simple wrapper of a graph that should be formatted when converted.
    /// </summary>
    /// <typeparam name="TVertex">The vertex type.</typeparam>
    /// <typeparam name="TEdge">The edge type.</typeparam>
    public class JsonGraph<TVertex, TEdge>
        where TVertex : IVertex<TEdge>
        where TEdge : IEdge
    {
        /// <summary>
        /// The vertices of the graph. 
        /// </summary>
        public HashSet<TVertex> Vertices { get; private init; } = new();

        /// <summary>
        /// Creates a new instance of the <see cref="JsonGraph{TVertex, TEdge}"/> class.
        /// </summary>
        /// <param name="graph">The graph to construct the instance from.</param>
        /// <returns>The graph.</returns>
        public async Task Initialize(Graph graph)
        {
            if (graph.Components.TryFind(out IEnumerableComponent<TVertex, TEdge> enumerableComponent))
            {
                await foreach (TVertex vertex in enumerableComponent.GetVertices())
                    Vertices.Add(vertex);
            }
        }
    }
}