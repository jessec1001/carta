using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using CartaCore.Data;

namespace CartaWeb.Models.Data
{
    /// <summary>
    /// Represents a function to create a graph from an options object.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <typeparam name="TOptions">The type of options.</typeparam>
    /// <returns>A graph data object.</returns>
    public delegate Graph OptionsDataResolverFunction<TOptions>(TOptions options) where TOptions : new();

    /// <summary>
    /// Represents a data resolver that creates an options object from the controller route data.
    /// </summary>
    /// <typeparam name="TOptions">The type of options.</typeparam>
    public class OptionsDataResolver<TOptions> : IDataResolver
        where TOptions : class, new()
    {
        /// <summary>
        /// The resolver function.
        /// </summary>
        private OptionsDataResolverFunction<TOptions> Resolver;

        /// <summary>
        /// Creates a new options data resolver with a specified controller and resolver.
        /// </summary>
        /// <param name="resolver">The function to resolve the data.</param>
        public OptionsDataResolver(OptionsDataResolverFunction<TOptions> resolver)
        {
            Resolver = resolver;
        }

        /// <inheritdoc />
        public async Task<Graph> GenerateAsync(ControllerBase controller, string resource)
        {
            // Load the options from the controller.
            TOptions options = new TOptions();
            await controller.TryUpdateModelAsync<TOptions>(options);

            // Return the results of the resolver.
            return Resolver(options);
        }
    }
}