using System.Collections.Generic;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a graph where vertices and their in-edges can be retrieved by an identifier.
    /// </summary>
    public interface IDynamicInGraph<out TVertex> : IDynamicGraph<TVertex> where TVertex : IVertex
    {
        /// <summary>
        /// Gets the parent vertices of a child vertex specified by an identifier.
        /// </summary>
        /// <param name="id">The child vertex identifier.</param>
        /// <returns>An enumerable of parent vertices.</returns>
        IAsyncEnumerable<TVertex> GetParentVertices(Identity id);
    }
}