using System.Collections.Generic;

namespace CartaCore.Graph.Components
{
    /// <summary>
    /// A graph component that allows retrieval of the children of a vertex.
    /// </summary>
    /// <typeparam name="TVertex">The type of vertex.</typeparam>
    /// <typeparam name="TEdge">The type of edge.</typeparam>
    public interface IDynamicOutComponent<out TVertex, out TEdge>
        where TVertex : IVertex<TEdge>
        where TEdge : IEdge
    {
        /// <summary>
        /// Gets the child vertices of a vertex specified by an identifier.
        /// </summary>
        /// <param name="id">The parent vertex identifier.</param>
        /// <returns>An enumerable of child vertices.</returns>
        IAsyncEnumerable<TVertex> GetChildVertices(string id);
    }
    /// <summary>
    /// A graph component that allows retrieval of the children of a vertex.
    /// </summary>
    public interface IDynamicOutComponent : IDynamicOutComponent<IVertex<IEdge>, IEdge> { }
}