using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CartaCore.Graphs;
using CartaCore.Operations;

namespace CartaWeb.Models.Data
{
    /// <summary>
    /// Represents a function to create a graph from an options object.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <typeparam name="TOptions">The type of options.</typeparam>
    /// <returns>A graph data object.</returns>
    public delegate Graph OptionsResourceResolverFunction<TOptions>(TOptions options) where TOptions : new();

    /// <summary>
    /// Represents an object that can convert a controllers parameters to an options object for a resource.
    /// </summary>
    public abstract class OptionsResourceResolver
    {
        /// <summary>
        /// Generates the data asynchronously.
        /// </summary>
        /// <param name="controller">The controller generating the request.</param>
        /// <returns>The freeform graph data.</returns>
        public abstract Task<Graph> GenerateAsync(ControllerBase controller);
        /// <summary>
        /// The operation template to use for retrieving the resource.
        /// </summary>
        public OperationTemplate Template { get; protected init; }

    }
    /// <summary>
    /// Represents an object that can convert a controllers parameters to an options object for a resource.
    /// </summary>
    /// <typeparam name="TOptions">The type of options to use.</typeparam>
    public class OptionsResourceResolver<TOptions> : OptionsResourceResolver where TOptions : class, new()
    {
        /// <summary>
        /// The resolver function.
        /// </summary>
        public OptionsResourceResolverFunction<TOptions> Resolver { get; protected init; }

        /// <summary>
        /// Initializes an instance of the <see cref="OptionsResourceResolver{TOptions}"/> class with the specified
        /// resource resolver.
        /// </summary>
        /// <param name="resolver">The resource resolver function.</param>
        /// <param name="template">The operation template to use for retrieving the resource.</param>
        public OptionsResourceResolver(OptionsResourceResolverFunction<TOptions> resolver, OperationTemplate template)
        {
            Resolver = resolver;
            Template = template;
        }

        /// <inheritdoc />
        public override async Task<Graph> GenerateAsync(ControllerBase controller)
        {
            // Load the options from the controller.
            TOptions options = new();
            await controller.TryUpdateModelAsync(options);

            // Return the results of the resolver.
            return Resolver(options);
        }
    }
}