using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CartaCore.Graphs;
using CartaCore.Operations;

namespace CartaWeb.Models.Data
{
    /// <summary>
    /// Represents a data resolver that creates an options object from the controller route data.
    /// </summary>
    public class OptionsDataResolver : IDataResolver
    {
        /// <summary>
        /// Gets the resolvers for each resource.
        /// </summary>
        /// <value>The map of resource keys to resolver values.</value>
        public Dictionary<string, OptionsResourceResolver> Resolvers { get; protected init; }

        /// <summary>
        /// Creates a new options data resolver with the specified resolvers.
        /// </summary>
        /// <param name="resolvers">The resolvers for each resource.</param>
        public OptionsDataResolver(Dictionary<string, OptionsResourceResolver> resolvers)
        {
            Resolvers = resolvers;
        }

        /// <inheritdoc />
        public async Task<Graph> GenerateGraphAsync(ControllerBase controller, string resource)
        {
            if (Resolvers.TryGetValue(resource, out OptionsResourceResolver resolver))
                return await resolver.GenerateAsync(controller);
            return null;
        }

        /// <inheritdoc />
        public Task<IList<string>> FindResourcesAsync(ControllerBase controller)
        {
            return Task.FromResult((IList<string>)Resolvers.Keys.ToList());
        }

        /// <inheritdoc />
        public Task<OperationTemplate> GenerateOperation(ControllerBase controller, string resource)
        {
            if (Resolvers.TryGetValue(resource, out OptionsResourceResolver resolver))
                return Task.FromResult(resolver.Template);
            return null;
        }
    }
}