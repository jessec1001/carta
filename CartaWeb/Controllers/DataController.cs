using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using QuikGraph;

using CartaCore.Data;
using CartaCore.Data.Synthetic;
using CartaCore.Integration.Hyperthought;

using CartaWeb.Models.Data;

namespace CartaWeb.Controllers
{
    using FreeformGraph = IMutableVertexAndEdgeSet<FreeformVertex, FreeformEdge>;

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
                    if (!(resource is null))
                    {
                        if (Request.Query.ContainsKey("api"))
                        {
                            HyperthoughtApi api = new HyperthoughtApi(Request.Query["api"].ToString());
                            return new HyperthoughtWorkflowGraph(api, Guid.Parse(resource));
                        }
                    }
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
                    // Generate a graph with the correct directed variant.
                    FreeformGraph data;
                    if (graph.IsDirected)
                        data = new AdjacencyGraph<FreeformVertex, FreeformEdge>(true, 1, 0);
                    else
                        data = new UndirectedGraph<FreeformVertex, FreeformEdge>(true);

                    // Add the single base vertex.
                    data.AddVertex(graph.GetProperties(graph.BaseId));
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
        /// <returns>A graph with the requested vertex loaded with itsproperties.</returns>
        [HttpGet("{source}/{resource?}/props")]
        public async Task<FreeformGraph> GetProperties(
            [FromRoute] DataSource source,
            [FromRoute] string resource,
            [FromQuery] Guid uuid
        )
        {
            ISampledGraph graph = await LookupData(source, resource);

            if (!(graph is null))
            {
                // Generate a graph with the correct directed variant.
                FreeformGraph data;
                if (graph.IsDirected)
                    data = new AdjacencyGraph<FreeformVertex, FreeformEdge>(true, 1, 0);
                else
                    data = new UndirectedGraph<FreeformVertex, FreeformEdge>(true);

                // Add the requested vertex.
                data.AddVertex(graph.GetProperties(uuid));

                // Simply return the properties of the vertex.
                return data;
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
        [HttpGet("{source}/{resource?}/children")]
        public async Task<FreeformGraph> GetChildren(
            [FromRoute] DataSource source,
            [FromRoute] string resource,
            [FromQuery] Guid uuid
        )
        {
            ISampledGraph graph = await LookupData(source, resource);

            if (!(graph is null))
            {
                FreeformGraph data = graph.GetChildrenWithEdges(uuid);
                return data;
            }
            return null;
        }
    }
}