using System.Collections.Generic;
using MorseCode.ITask;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a graph where vertices can be retrieved by an identifier.
    /// </summary>
    public interface IDynamicGraph<out TVertex, out TEdge> : IGraph
        where TVertex : IVertex<TEdge>
        where TEdge : IEdge
    {
        /// <summary>
        /// Gets a vertex specified by an identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The retrieved vertex.</returns>
        ITask<TVertex> GetVertex(string id);
        /// <summary>
        /// Gets a collection of vertices specified by identifiers.
        /// </summary>
        /// <param name="ids">The identifiers.</param>
        /// <returns>The retrieved vertices.</returns>
        async IAsyncEnumerable<TVertex> GetVertices(IEnumerable<string> ids)
        {
            // This default implementation is extremely simple so in most cases it should not be overridden.
            foreach (string id in ids)
                yield return await GetVertex(id);
        }
    }
}