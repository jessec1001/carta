using System.Collections.Generic;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a graph where vertices and their in-edges can be retrieved by an identifier.
    /// </summary>
    public interface IDynamicInGraph<out TVertex> : IDynamicGraph<TVertex> where TVertex : IVertex, IInVertex
    {
        /// <summary>
        /// Gets the parent vertices of a child vertex specified by an identifier.
        /// </summary>
        /// <param name="id">The child vertex identifier.</param>
        /// <returns>An enumerable of parent vertices.</returns>
        async IAsyncEnumerable<TVertex> GetParentVertices(Identity id)
        {
            // This default implementation is extremely simple so in most cases it should not be overridden.
            TVertex vertex = await GetVertex(id);
            foreach (Edge inEdge in vertex.InEdges)
                yield return await GetVertex(inEdge.Source);
        }
    }
}