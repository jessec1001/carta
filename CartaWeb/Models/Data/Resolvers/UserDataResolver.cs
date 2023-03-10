using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CartaCore.Graphs;
using CartaCore.Operations;
using CartaWeb.Controllers;

namespace CartaWeb.Models.Data
{
    /// <summary>
    /// Represents a data resolver that retrieves User-stored data from the local file system.
    /// </summary>
    public class UserDataResolver : IDataResolver
    {
        /// <inheritdoc />
        public async Task<IList<string>> FindResourcesAsync(ControllerBase controller)
        {
            // TODO: Optimize loading of user-uploaded graphs so that the aliases and identifiers are loaded but not the graph itself.
            List<MemoryGraph> graphs = await DataController.LoadGraphsAsync();
            return graphs
                .Select(graph => graph.Id)
                .ToList();
        }

        /// <inheritdoc />
        public async Task<Graph> GenerateGraphAsync(ControllerBase controller, string resource)
        {
            if (!int.TryParse(resource, out int id)) return null;
            return await DataController.LoadGraphAsync(id);
        }

        /// <inheritdoc />
        public Task<OperationTemplate> GenerateOperation(ControllerBase controller, string resource)
        {
            // TODO: This needs to be reimplemented after token authentication is implemented.
            return Task.FromResult(new OperationTemplate());
        }
    }
}