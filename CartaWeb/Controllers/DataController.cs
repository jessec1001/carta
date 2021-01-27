using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using QuikGraph;

using CartaCore.Data;
using CartaCore.Data.Synthetic;

using CartaWeb.Models.Data;

namespace CartaWeb.Controllers
{
    using FreeformGraph = IEdgeListAndIncidenceGraph<FreeformVertex, Edge<FreeformVertex>>;

    /// <summary>
    /// Serves data from multiple sources in graph format.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DataController : ControllerBase
    {
        private readonly ILogger<DataController> _logger;

        private Dictionary<string, IDataResolver> SyntheticResolvers;

        /// <inheritdoc />
        public DataController(ILogger<DataController> logger)
        {
            _logger = logger;

            SyntheticResolvers = new Dictionary<string, IDataResolver>()
            {
                [nameof(RandomFiniteUndirectedGraph).ToLower()] = new OptionsDataResolver<RandomFiniteUndirectedGraphOptions>
                    (this, options => new RandomFiniteUndirectedGraph(options)),
                [nameof(RandomInfiniteDirectedGraph).ToLower()] = new OptionsDataResolver<RandomInfiniteDirectedGraphOptions>
                    (this, options => new RandomInfiniteDirectedGraph(options))
            };
        }

        /// <summary>
        /// Finds the sample graph associated with a specified source and resource.
        /// </summary>
        /// <param name="source">The data source.</param>
        /// <param name="resource">The resource located on the data source</param>
        /// <returns>The graph data.</returns>
        private async Task<ISampledGraph> LookupData(DataSource source, string resource)
        {
            switch (source)
            {
                case DataSource.Synthetic:
                    if (!(resource is null) && SyntheticResolvers.TryGetValue(resource.ToLower(), out IDataResolver resolver))
                    {
                        return await resolver.GenerateAsync();
                    }
                    break;
                case DataSource.HyperThought:
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
        public async Task<FreeformGraph> GetGraph(
            [FromRoute] DataSource source,
            [FromRoute] string resource
        )
        {
            ISampledGraph graph = await LookupData(source, resource);

            if (!(graph is null))
            {
                if (graph.IsFinite)
                {
                    // Generate the entire graph.
                    FreeformGraph data = graph.GetEntire();
                    return data;
                }
                else
                {
                    // Generate random starting node ID.
                    Guid nodeId = Guid.NewGuid();

                    // Generate graph with starting node.
                    AdjacencyGraph<FreeformVertex, Edge<FreeformVertex>> data = new AdjacencyGraph<FreeformVertex, Edge<FreeformVertex>>();
                    data.AddVertex(graph.GetProperties(nodeId));
                    return data;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the properties of a specific vertex for a particular data source.
        /// </summary>
        /// <param name="source">The data source.</param>
        /// <param name="resource">The resource located on the data source.</param>
        /// <param name="uuid">The UUID of the vertex.</param>
        /// <returns>The vertex with its properties loaded.</returns>
        [HttpGet("props/{source}/{resource?}")]
        public async Task<FreeformVertex> GetProperties(
            [FromRoute] DataSource source,
            [FromRoute] string resource,
            [FromQuery] Guid uuid
        )
        {
            ISampledGraph graph = await LookupData(source, resource);

            if (!(graph is null))
            {
                // Simply return the properties of the vertex.
                return graph.GetProperties(uuid);
            }
            return null;
        }

        /// <summary>
        /// Gets the children vertices of a specific vertex for a particular data source.
        /// </summary>
        /// <param name="source">The data source.</param>
        /// <param name="resource">The resource located on the data source.</param>
        /// <param name="uuid">The UUID of the vertex.</param>
        /// <returns>The vertex with its properties loaded.</returns>
        [HttpGet("children/{source}/{resource?}")]
        public async Task<IDictionary<Guid, FreeformVertex>> GetChildren(
            [FromRoute] DataSource source,
            [FromRoute] string resource,
            [FromQuery] Guid uuid
        )
        {
            ISampledGraph graph = await LookupData(source, resource);

            if (!(graph is null))
            {
                // Return a dictionary of node ID, node properties pairs.
                return graph.GetEdges(uuid)
                    .ToDictionary(
                        edge => edge.Target.Id,
                        edge => graph.GetProperties(edge.Target.Id)
                    );
            }
            return null;
        }
    }
}