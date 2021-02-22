using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using CartaCore.Data.Freeform;
using CartaCore.Data.Synthetic;
using CartaCore.Workflow.Selection;

using CartaWeb.Models.Data;
using CartaWeb.Models.Selections;

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
        private async Task<FreeformGraph> LookupData(DataSource source, string resource)
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
        public async Task<ActionResult<FreeformGraph>> GetGraph(
            [FromRoute] DataSource source,
            [FromRoute] string resource
        )
        {
            FreeformGraph graph = await LookupData(source, resource);

            if (graph is null)
            {
                // We could not find the resource, we should return a not found response.
                return NotFound();
            }
            else
            {
                if (graph is FreeformDynamicGraph dynamicGraph)
                {
                    // Return the subgraph of the graph containing the base vertex.
                    FreeformFiniteGraph subgraph = FreeformFiniteGraph.CreateSubgraph
                    (
                        dynamicGraph,
                        new[] { dynamicGraph.BaseId },
                        includeEdges: true
                    );
                    return Ok(subgraph);
                }
                else
                {
                    // Return the entire graph.
                    return Ok(graph);
                }
            }
        }

        /// <summary>
        /// Gets the properties of a specific vertex for a particular data source.
        /// </summary>
        /// <param name="source">The data source.</param>
        /// <param name="resource">The resource located on the data source.</param>
        /// <param name="uuid">The UUID of the vertex.</param>
        /// <returns>A graph with the requested vertex loaded with itsproperties.</returns>
        [HttpGet("{source}/{resource?}/props")]
        public async Task<ActionResult<FreeformGraph>> GetProperties(
            [FromRoute] DataSource source,
            [FromRoute] string resource,
            [FromQuery] string uuid
        )
        {
            FreeformGraph graph = await LookupData(source, resource);

            if (graph is null)
            {
                // We could not find the resource, we should return a not found response.
                return NotFound();
            }
            else
            {
                if (graph is FreeformDynamicGraph dynamicGraph)
                {
                    // Return the subgraph of the graph containing the requested vertex.
                    FreeformFiniteGraph subgraph = FreeformFiniteGraph.CreateSubgraph
                    (
                        dynamicGraph,
                        new[] { FreeformIdentity.Create(uuid) },
                        includeEdges: true
                    );
                    return Ok(subgraph);
                }
                else
                {
                    // Requesting the properties of a non-dynamic graph is a bad request.
                    return BadRequest();
                }
            }
        }

        /// <summary>
        /// Gets the children vertices of a specific vertex for a particular data source.
        /// </summary>
        /// <param name="source">The data source.</param>
        /// <param name="resource">The resource located on the data source.</param>
        /// <param name="uuid">The UUID of the vertex.</param>
        /// <returns>The vertex with its properties loaded.</returns>
        [HttpGet("{source}/{resource?}/children")]
        public async Task<ActionResult<FreeformGraph>> GetChildren(
            [FromRoute] DataSource source,
            [FromRoute] string resource,
            [FromQuery] string uuid
        )
        {
            FreeformGraph graph = await LookupData(source, resource);

            if (graph is null)
            {
                // We could not find the resource, we should return a not found response.
                return NotFound();
            }
            else
            {
                if (graph is FreeformDynamicGraph dynamicGraph)
                {
                    // Return the subgraph of the graph containing the requested vertex.
                    FreeformFiniteGraph subgraph = FreeformFiniteGraph.CreateChildSubgraph
                    (
                        dynamicGraph,
                        new[] { FreeformIdentity.Create(uuid) },
                        includeEdges: true
                    );
                    return Ok(subgraph);
                }
                else
                {
                    // Requesting the properties of a non-dynamic graph is a bad request.
                    return BadRequest();
                }
            }
        }

        /// <summary>
        /// Gets the selected vertices from a subset of vertices specified by the application of a series of selectors.
        /// </summary>
        /// <remarks>
        /// This endpoint is created for ease of performing a single selection or iterative selection. The
        /// <code>ids</code> field should be specified to refine to refine the search. If <code>ids</code> is not
        /// specified, the selection is performed on the entire graph.  
        /// </remarks>
        /// <example>
        /// <code>
        /// // Iterative selection.
        /// ids = GET /api/data/source/resource/selection
        ///       BODY { selectors: [...] }
        /// ids = GET /api/data/source/resource/selection
        ///       BODY { selectors: [...], ids }
        /// ids = GET /api/data/source/resource/selection
        ///       BODY { selectors: [...], ids }
        /// </code>
        /// </example>
        /// <param name="source">The data source.</param>
        /// <param name="resource">The resource located on the data source.</param>
        /// <param name="request">
        /// The selection request. Made up of the identifiers to get information on and the stack of selectors to apply.
        /// </param>
        /// <returns></returns>
        [HttpPost("{source}/{resource?}/select")]
        public async Task<IEnumerable<string>> GetSelection(
            [FromRoute] DataSource source,
            [FromRoute] string resource,
            [FromBody] SelectionRequest request
        )
        {
            FreeformGraph graph = await LookupData(source, resource);
            FreeformDynamicGraph dynamicGraph = graph as FreeformDynamicGraph;

            // When no identifiers are specified, we simply retrieve all identifiers that match the selection.
            FreeformFiniteGraph subgraph;
            if (request.Ids is null)
            {
                if (dynamicGraph is null)
                    subgraph = FreeformFiniteGraph.CreateSubgraph(graph, null, includeEdges: false);
                else
                    subgraph = FreeformFiniteGraph.CreateSubgraph(dynamicGraph, null, includeEdges: false);
            }
            else
            {
                IEnumerable<FreeformIdentity> includedIds = request.Ids.Select(id => FreeformIdentity.Create(id));
                if (dynamicGraph is null)
                    subgraph = FreeformFiniteGraph.CreateSubgraph(graph, includedIds, includeEdges: false);
                else
                    subgraph = FreeformFiniteGraph.CreateSubgraph(dynamicGraph, includedIds, includeEdges: false);
            }
            foreach (SelectorBase selector in request.Selectors)
            {
                subgraph = FreeformFiniteGraph.CreateSubgraph(subgraph,
                    subgraph.Vertices
                        .Where(vertex => selector.Contains(vertex))
                        .Select(vertex => vertex.Identifier),
                    includeEdges: false
                );
            }
            return subgraph.Vertices.Select(vertex => vertex.Identifier.ToString());
        }
    }
}