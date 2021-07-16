using System.Collections.Generic;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents the base functionality of a vertex.
    /// </summary>
    public interface IVertex : IElement<Vertex>
    {
        /// <summary>
        /// Gets all of the in- and out-edges of the specified vertex.
        /// </summary>
        /// <param name="vertex">The vertex to get the edges of.</param>
        /// <returns>An enumerable of all edges of the vertex.</returns>
        static IEnumerable<Edge> GetEdges(IVertex vertex)
        {
            if (vertex is IInVertex inVertex)
            {
                foreach (Edge edge in inVertex.InEdges)
                    yield return edge;
            }
            if (vertex is IOutVertex outVertex)
            {
                foreach (Edge edge in outVertex.OutEdges)
                    yield return edge;
            }
        }
    }
}