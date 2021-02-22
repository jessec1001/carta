using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using CartaCore.Data.Freeform;
using CartaCore.Integration.Hyperthought;

namespace CartaWeb.Models.Data
{
    /// <summary>
    /// Represents a data resolver that retrieves HyperThought data from the controller route data.
    /// </summary>
    public class HyperthoughtDataResolver : IDataResolver
    {
        private static readonly int AliasCount = short.MaxValue;

        private SortedList<string, Guid> Aliases;

        /// <summary>
        /// Creates a new instance of the <see cref="HyperthoughtDataResolver"/> class with a specified controller.
        /// </summary>
        public HyperthoughtDataResolver()
        {
            Aliases = new SortedList<string, Guid>(AliasCount, StringComparer.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public async Task<FreeformGraph> GenerateAsync(ControllerBase controller, string resource)
        {
            if (controller.Request.Query.ContainsKey("api"))
            {
                // We need to find the UUID associated with a HyperThought resource if we are passed an alias.
                HyperthoughtApi api = new HyperthoughtApi(controller.Request.Query["api"].ToString());
                Guid uuid = Guid.Empty;
                if (!Guid.TryParse(resource, out uuid))
                {
                    // Find the alias if we don't currently have it.
                    if (!Aliases.TryGetValue(resource, out uuid))
                    {
                        // Get the alias.
                        uuid = await api.GetWorkflowIdFromPathAsync(resource);
                        if (uuid == Guid.Empty) return null;

                        // Make sure we don't use too many aliases and add the new alias.
                        if (Aliases.Count >= AliasCount) Aliases.Clear();
                        Aliases.Add(resource, uuid);
                    }
                }
                return new HyperthoughtWorkflowGraph(api, uuid);
            }
            return null;
        }
    }
}