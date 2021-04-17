using System.Collections.Generic;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents the base functionality of a vertex.
    /// </summary>
    public interface IVertex : IElement<Vertex>
    {
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