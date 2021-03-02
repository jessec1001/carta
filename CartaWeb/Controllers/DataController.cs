using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using CartaCore.Data;
using CartaCore.Data.Synthetic;
using CartaWeb.Models.Data;

namespace CartaWeb.Controllers
{
    /// <summary>
    /// Serves data from multiple sources in graph format.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DataController : ControllerBase
    {
        private readonly ILogger<DataController> _logger;

        private static Dictionary<string, IDataResolver> SyntheticResolvers;
        private static IDataResolver HyperthoughtResolver;

        static DataController()
        {
            // Create the data resolvers.
            SyntheticResolvers = new Dictionary<string, IDataResolver>
            (
                new Dictionary<string, IDataResolver>
                {
                    [nameof(FiniteUndirectedGraph)] = new OptionsDataResolver<FiniteUndirectedGraphParameters>
                        (options => new FiniteUndirectedGraph(options)),
                    [nameof(InfiniteDirectedGraph)] = new OptionsDataResolver<InfiniteDirectedGraphParameters>
                        (options => new InfiniteDirectedGraph(options)),
                },
                StringComparer.OrdinalIgnoreCase
            );
            HyperthoughtResolver = new HyperthoughtDataResolver();
        }

        /// <inheritdoc />
        public DataController(ILogger<DataController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Finds the sample graph associated with a specified source and resource.
        /// </summary>
        /// <param name="source">The data source.</param>
        /// <param name="resource">The resource located on the data source</param>
        /// <returns>The graph data.</returns>
        private async Task<Graph> LookupData(DataSource source, string resource)
        {
            switch (source)
            {
                case DataSource.Synthetic:
                    if (!(resource is null) && SyntheticResolvers.TryGetValue(resource, out IDataResolver resolver))
                        return await resolver.GenerateAsync(this, resource);
                    break;
                case DataSource.HyperThought:
                    if (!(resource is null))
                        return await HyperthoughtResolver.GenerateAsync(this, resource);
                    break;
            }
            return null;
        }

        /// <summary>
        /// Gets the base graph for a particular data source.
        /// </summary>
        /// <remarks>
        /// If the data is infinite, a graph with only vertex is returned. Otherwise, the entire graph is returned. 
        /// </remarks>
        /// <param name="source">The data source.</param>
        /// <param name="resource">The resource located on the data source.</param>
        /// <returns>The base graph.</returns>
        [HttpGet("{source}/{resource?}")]
        public async Task<ActionResult<FiniteGraph>> GetGraph(
            [FromRoute] DataSource source,
            [FromRoute] string resource
        )
        {
            Graph graph = await LookupData(source, resource);

            // If we could not find the resource, we should return a not found response.
            if (graph is null) return NotFound();

            // We return a response based on the type of graph.
            FiniteGraph subgraph;
            switch (graph)
            {
                case IDynamicOutGraph<IOutVertex> outGraph:
                    subgraph = await FiniteGraph.CreateSubgraph(outGraph, new[] { outGraph.BaseIdentifier });
                    return Ok(subgraph);
                case IDynamicGraph<Vertex> dynGraph:
                    // Return the subgraph of the graph containing the base vertex.
                    subgraph = await FiniteGraph.CreateSubgraph(dynGraph, new[] { dynGraph.BaseIdentifier });
                    return Ok(subgraph);
                default:
                    // If a graph does not match a previous type, we cannot retrieve it.
                    return BadRequest();
            }
        }

        /// <summary>
        /// Gets the properties of a specific vertex for a particular data source.
        /// </summary>
        /// <param name="source">The data source.</param>
        /// <param name="resource">The resource located on the data source.</param>
        /// <param name="id">The UUID of the vertex.</param>
        /// <returns>A graph with the requested vertex loaded with itsproperties.</returns>
        [HttpGet("{source}/{resource?}/props")]
        public async Task<ActionResult<FiniteGraph>> GetProperties(
            [FromRoute] DataSource source,
            [FromRoute] string resource,
            [FromQuery] string id
        )
        {
            Graph graph = await LookupData(source, resource);

            // If we could not find the resource, we should return a not found response.
            if (graph is null) return NotFound();

            // We return a response based on the type of graph.
            FiniteGraph subgraph;
            switch (graph)
            {
                case IDynamicOutGraph<IOutVertex> outGraph:
                    // Return the subgraph of the graph containing the requested vertex.
                    subgraph = await FiniteGraph.CreateSubgraph(outGraph, new[] { Identity.Create(id) });
                    return Ok(subgraph);
                case IDynamicGraph<Vertex> dynGraph:
                    // Return the subgraph of the graph containing the base vertex.
                    subgraph = await FiniteGraph.CreateSubgraph(dynGraph, new[] { Identity.Create(id) });
                    return Ok(subgraph);
                default:
                    // If a graph does not match a previous type, we cannot retrieve it.
                    return BadRequest();
            }
        }

        /// <summary>
        /// Gets the children vertices of a specific vertex for a particular data source.
        /// </summary>
        /// <param name="source">The data source.</param>
        /// <param name="resource">The resource located on the data source.</param>
        /// <param name="id">The UUID of the vertex.</param>
        /// <returns>The vertex with its properties loaded.</returns>
        [HttpGet("{source}/{resource?}/children")]
        public async Task<ActionResult<Graph>> GetChildren(
            [FromRoute] DataSource source,
            [FromRoute] string resource,
            [FromQuery] string id
        )
        {
            Graph graph = await LookupData(source, resource);

            // If we could not find the resource, we should return a not found response.
            if (graph is null) return NotFound();

            // We return a response based on the type of graph.
            FiniteGraph subgraph;
            switch (graph)
            {
                case IDynamicOutGraph<InOutVertex> inoutGraph:
                    // Return the subgraph of the graph containing the requested vertex.
                    subgraph = await FiniteGraph.CreateChildSubgraph(inoutGraph, new[] { Identity.Create(id) });
                    return Ok(subgraph);
                case IDynamicOutGraph<OutVertex> outGraph:
                    // Return the subgraph of the graph containing the requested vertex.
                    subgraph = await FiniteGraph.CreateChildSubgraph(outGraph, new[] { Identity.Create(id) });
                    return Ok(subgraph);
                default:
                    // If a graph does not match a previous type, we cannot retrieve it.
                    return BadRequest();
            }
        }

        // /// <summary>
        // /// Gets the selected vertices from a subset of vertices specified by the application of a series of selectors.
        // /// </summary>
        // /// <remarks>
        // /// This endpoint is created for ease of performing a single selection or iterative selection. The
        // /// <code>ids</code> field should be specified to refine to refine the search. If <code>ids</code> is not
        // /// specified, the selection is performed on the entire graph.  
        // /// </remarks>
        // /// <example>
        // /// <code>
        // /// // Iterative selection.
        // /// ids = GET /api/data/source/resource/selection
        // ///       BODY { selectors: [...] }
        // /// ids = GET /api/data/source/resource/selection
        // ///       BODY { selectors: [...], ids }
        // /// ids = GET /api/data/source/resource/selection
        // ///       BODY { selectors: [...], ids }
        // /// </code>
        // /// </example>
        // /// <param name="source">The data source.</param>
        // /// <param name="resource">The resource located on the data source.</param>
        // /// <param name="request">
        // /// The selection request. Made up of the identifiers to get information on and the stack of selectors to apply.
        // /// </param>
        // /// <returns></returns>
        // [HttpPost("{source}/{resource?}/select")]
        // public async Task<IEnumerable<string>> GetSelection(
        //     [FromRoute] DataSource source,
        //     [FromRoute] string resource,
        //     [FromBody] SelectionRequest request
        // )
        // {
        //     Graph graph = await LookupData(source, resource);
        //     FreeformDynamicGraph dynamicGraph = graph as FreeformDynamicGraph;

        //     // When no identifiers are specified, we simply retrieve all identifiers that match the selection.
        //     FreeformFiniteGraph subgraph;
        //     if (request.Ids is null)
        //     {
        //         if (dynamicGraph is null)
        //             subgraph = FreeformFiniteGraph.CreateSubgraph(graph, null, includeEdges: false);
        //         else
        //             subgraph = FreeformFiniteGraph.CreateSubgraph(dynamicGraph, null, includeEdges: false);
        //     }
        //     else
        //     {
        //         IEnumerable<Identity> includedIds = request.Ids.Select(id => Identity.Create(id));
        //         if (dynamicGraph is null)
        //             subgraph = FreeformFiniteGraph.CreateSubgraph(graph, includedIds, includeEdges: false);
        //         else
        //             subgraph = FreeformFiniteGraph.CreateSubgraph(dynamicGraph, includedIds, includeEdges: false);
        //     }
        //     foreach (SelectorBase selector in request.Selectors)
        //     {
        //         subgraph = FreeformFiniteGraph.CreateSubgraph(subgraph,
        //             subgraph.Vertices
        //                 .Where(vertex => selector.Contains(vertex))
        //                 .Select(vertex => vertex.Identifier),
        //             includeEdges: false
        //         );
        //     }
        //     return subgraph.Vertices.Select(vertex => vertex.Identifier.ToString());
        // }
    }
}