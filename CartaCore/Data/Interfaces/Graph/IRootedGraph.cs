using System.Collections.Generic;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a graph that has specified roots with in-degree zero that can be used to improve retrieval speed.
    /// </summary>
    public interface IRootedGraph : IGraph
    {
        /// <summary>
        /// Gets an enumerable of the identities of root vertices.
        /// </summary>
        /// <returns>The identities of root vertices.</returns>
        IEnumerable<Identity> GetRoots();
    }
}