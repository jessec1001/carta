using System.Collections.Generic;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a graph where vertices and their out-edges can be retrieved by an identifier.
    /// </summary>
    public interface IDynamicOutGraph<out TVertex> : IDynamicGraph<TVertex> where TVertex : IVertex, IOutVertex
    {
        /// <summary>
        /// Gets the child vertices of a parent vertex specified by an identifier.
        /// </summary>
        /// <param name="id">The parent vertex identifier.</param>
        /// <returns>An enumerable of child vertices.</returns>
        async IAsyncEnumerable<TVertex> GetChildVertices(Identity id)
        {
            // This default implementation is extremely simple so in most cases it should not be overridden.
            TVertex vertex = await GetVertex(id);
            foreach (Edge outEdge in vertex.OutEdges)
                yield return await GetVertex(outEdge.Target);
        }
    }
}