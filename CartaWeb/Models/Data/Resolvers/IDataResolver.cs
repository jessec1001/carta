using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CartaCore.Graphs;
using CartaCore.Operations;

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
        Task<Graph> GenerateGraphAsync(ControllerBase controller, string resource);

        /// <summary>
        /// Generates an operation template that may used to construct an operation to retrieve the resource.
        /// </summary>
        /// <param name="controller">The controller generating the request.</param>
        /// <param name="resource">The resource to generate.</param>
        /// <returns>The operation template.</returns>
        Task<OperationTemplate> GenerateOperation(ControllerBase controller, string resource);

        /// <summary>
        /// Finds all the resources associated with the data resolver.
        /// </summary>
        /// <returns>The list of resource names.</returns>
        Task<IList<string>> FindResourcesAsync(ControllerBase controller);
    }
}