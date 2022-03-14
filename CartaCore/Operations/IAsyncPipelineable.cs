using System.Collections.Generic;
using System.Threading.Tasks;

namespace CartaCore.Operations
{
    /// <summary>
    /// Represents an
    /// </summary>
    /// <typeparam name="TPipelineable"></typeparam>
    /// <typeparam name="TStructure"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    public interface IAsyncPipelineable<TPipelineable, TStructure, TElement>
        where TPipelineable : IAsyncPipelineable<TPipelineable, TStructure, TElement>
    {
        /// <summary>
        /// Deconstructs a structure.
        /// </summary>
        /// <returns>The structure of the pipelineable object.</returns>
        TStructure Deconstruct();
        /// <summary>
        /// Enumerates a structure from its contituent elements.
        /// </summary>
        /// <returns>An enumeration of the elements in the structure.</returns>
        IAsyncEnumerable<TElement> Enumerate();
        /// <summary>
        /// Renumerates a structure from its constituent elements.
        /// </summary>
        /// <param name="elements">An enumeration of elements in the structure.</param>
        /// <param name="structure">The structure of the pipelineable object.</param>
        /// <returns>A structure constructed from the elements.</returns>
        Task<TPipelineable> Renumerate(IAsyncEnumerable<TElement> elements, TStructure structure);
    }
}