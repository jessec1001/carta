using System.Collections.Generic;
using System.Threading.Tasks;

namespace CartaCore.Operations
{
    /// <summary>
    /// Represents the functionality of a structure that can be updated asynchronously after its initial construction.
    /// </summary>
    public interface IUpdateable
    {
        /// <summary>
        /// Attempts to update the structure and returns a value indicating if there are more updates available.
        /// </summary>
        /// <returns><c>true</c> if an update occurred; <c>false</c> otherwise.</returns>
        Task<bool> UpdateAsync();
    }
}