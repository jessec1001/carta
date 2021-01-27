using System.Threading.Tasks;

using CartaCore.Data;

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
        /// <returns>The sampled graph data.</returns>
        Task<ISampledGraph> GenerateAsync();
    }
}