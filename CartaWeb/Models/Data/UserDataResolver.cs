using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using CartaCore.Data;
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
            List<FiniteGraph> graphs = await DataController.LoadGraphsAsync();
            return graphs
                .Select(graph => graph.Identifier.ToString())
                .ToList();
        }

        /// <inheritdoc />
        public async Task<Graph> GenerateAsync(ControllerBase controller, string resource)
        {
            if (!int.TryParse(resource, out int id)) return null;
            return await DataController.LoadGraphAsync(id);
        }
    }
}