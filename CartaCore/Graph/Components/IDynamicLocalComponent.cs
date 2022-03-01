using System.Collections.Generic;
using MorseCode.ITask;

namespace CartaCore.Graphs.Components
{
    /// <summary>
    /// A graph component that allows retrieval of a vertex specified by an identifier.
    /// </summary>
    /// <typeparam name="TVertex">The type of vertex.</typeparam>
    /// <typeparam name="TEdge">The type of edge.</typeparam>
    public interface IDynamicLocalComponent<out TVertex, out TEdge>
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
            foreach (string id in ids)
                yield return await GetVertex(id);
        }
    }
    /// <summary>
    /// A graph component that allows retrieval of a vertex specified by an identifier.
    /// </summary>
    public interface IDynamicLocalComponent : IDynamicOutComponent<IVertex<IEdge>, IEdge> { }
}