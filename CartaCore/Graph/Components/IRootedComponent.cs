using System.Collections.Generic;

namespace CartaCore.Graphs.Components
{
    /// <summary>
    /// A graph component that allows retrieval of the root vertex identifiers.
    /// </summary>
    public interface IRootedComponent
    {
        /// <summary>
        /// Enumerates over the identities of root vertices.
        /// </summary>
        /// <returns>The identities of root vertices.</returns>
        IAsyncEnumerable<string> Roots();
    }
}