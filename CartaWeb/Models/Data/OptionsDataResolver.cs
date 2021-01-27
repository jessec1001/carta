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
    public delegate ISampledGraph OptionsDataResolverFunction<TOptions>(TOptions options) where TOptions : new();

    /// <summary>
    /// Represents a data resolver that creates an options object from the controller route data.
    /// </summary>
    /// <typeparam name="TOptions">The type of options.</typeparam>
    public class OptionsDataResolver<TOptions> : IDataResolver
        where TOptions : class, new()
    {
        /// <summary>
        /// The controller generating the request.
        /// </summary>
        private ControllerBase Controller;
        /// <summary>
        /// The resolver function.
        /// </summary>
        private OptionsDataResolverFunction<TOptions> Resolver;

        /// <summary>
        /// Creates a new options data resolver with a specified controller and resolver.
        /// </summary>
        /// <param name="controller">The controller used to generate the data request.</param>
        /// <param name="resolver">The function to resolve the data.</param>
        public OptionsDataResolver(
            ControllerBase controller,
            OptionsDataResolverFunction<TOptions> resolver
        )
        {
            Controller = controller;
            Resolver = resolver;
        }

        /// <inheritdoc />
        public async Task<ISampledGraph> GenerateAsync()
        {
            // Load the options from the controller.
            TOptions options = new TOptions();
            await Controller.TryUpdateModelAsync<TOptions>(options);

            // Return the results of the resolver.
            return Resolver(options);
        }
    }
}