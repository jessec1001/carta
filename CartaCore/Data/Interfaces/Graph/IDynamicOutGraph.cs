using System.Collections.Generic;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a graph where vertices and their out-edges can be retrieved by an identifier.
    /// </summary>
    public interface IDynamicOutGraph<out TVertex, out TEdge> : IDynamicGraph<TVertex, TEdge>
        where TVertex : IVertex<TEdge>
        where TEdge : IEdge
    {
        /// <summary>
        /// Gets the child vertices of a parent vertex specified by an identifier.
        /// </summary>
        /// <param name="id">The parent vertex identifier.</param>
        /// <returns>An enumerable of child vertices.</returns>
        IAsyncEnumerable<TVertex> GetChildVertices(string id);
    }
}