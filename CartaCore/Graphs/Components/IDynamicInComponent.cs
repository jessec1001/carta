using System.Collections.Generic;

namespace CartaCore.Graphs.Components
{
    /// <summary>
    /// A graph component that allows retrieval of the parents of a vertex.
    /// </summary>
    /// <typeparam name="TVertex">The type of vertex.</typeparam>
    /// <typeparam name="TEdge">The type of edge.</typeparam>
    public interface IDynamicInComponent<out TVertex, out TEdge> : IComponent
        where TVertex : IVertex<TEdge>
        where TEdge : IEdge
    {
        /// <summary>
        /// Gets the parent vertices of a vertex specified by an identifier.
        /// </summary>
        /// <param name="id">The child vertex identifier.</param>
        /// <returns>An enumerable of parent vertices.</returns>
        IAsyncEnumerable<TVertex> GetParentVertices(string id);
    }
    /// <summary>
    /// A graph component that allows retrieval of the parents of a vertex.
    /// </summary>
    public interface IDynamicInComponent : IDynamicInComponent<IVertex<IEdge>, IEdge> { }
}