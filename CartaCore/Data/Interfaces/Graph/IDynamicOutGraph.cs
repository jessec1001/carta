using System.Collections.Generic;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a graph where vertices and their out-edges can be retrieved by an identifier.
    /// </summary>
    public interface IDynamicOutGraph<out TVertex> : IDynamicGraph<TVertex> where TVertex : IVertex
    {
        /// <summary>
        /// Gets the child vertices of a parent vertex specified by an identifier.
        /// </summary>
        /// <param name="id">The parent vertex identifier.</param>
        /// <returns>An enumerable of child vertices.</returns>
        IAsyncEnumerable<TVertex> GetChildVertices(Identity id);
    }
}