using System.Threading.Tasks;

using CartaCore.Data.Freeform;

namespace CartaWeb.Models.Data
{
    /// <summary>
    /// Resolves an function to its data source.
    /// </summary>
    public interface IDataResolver
    {
        /// <summary>
        /// Generates the data asynchronously.
        /// </summary>
        /// <returns>The freeform graph data.</returns>
        Task<IFreeformGraph> GenerateAsync();
    }
}