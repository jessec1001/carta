using System.Threading.Tasks;

using CartaCore.Data.Freeform;
using Microsoft.AspNetCore.Mvc;

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
        /// <param name="controller">The controller generating the request.</param>
        /// <param name="resource">The resource to generate.</param>
        /// <returns>The freeform graph data.</returns>
        Task<FreeformGraph> GenerateAsync(ControllerBase controller, string resource);
    }
}