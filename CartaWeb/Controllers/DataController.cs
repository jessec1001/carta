using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using QuikGraph;

using CartaCore.Data;
using CartaCore.Data.Synthetic;

namespace CartaWeb.Controllers
{
    using FreeformGraph = IEdgeListAndIncidenceGraph<FreeformVertex, Edge<FreeformVertex>>;

    [ApiController]
    [Route("api/[controller]")]
    public class DataController : ControllerBase
    {
        private readonly ILogger<DataController> _logger;

        public DataController(ILogger<DataController> logger)
        {
            _logger = logger;
        }

        [HttpGet("synthetic")]
        public FreeformGraph GetSynthetic(
            [FromQuery] ulong seed = 0
        )
        {
            // Generate and return graph.
            Guid nodeId = Guid.NewGuid();

            RandomInfiniteDirectedGraph data = new RandomInfiniteDirectedGraph(seed: seed, propertyCount: 50, childProbability: 1.0, propertyDensity: 0.10);
            AdjacencyGraph<FreeformVertex, Edge<FreeformVertex>> graph = new AdjacencyGraph<FreeformVertex, Edge<FreeformVertex>>();
            graph.AddVertex(data.GetProperties(nodeId));

            return graph;
        }

        [HttpGet("synthetic/props")]
        public FreeformVertex GetSyntheticProperties(
            [FromQuery] string id,
            [FromQuery] ulong seed = 0
        )
        {
            // Get the GUID.
            if (Guid.TryParse(id, out Guid nodeId))
            {
                RandomInfiniteDirectedGraph data = new RandomInfiniteDirectedGraph(seed: seed, propertyCount: 50, childProbability: 1.0, propertyDensity: 0.10);
                FreeformVertex node = data.GetProperties(nodeId);

                return node;
            }
            else
                throw new FormatException();
        }

        [HttpGet("synthetic/children")]
        public IDictionary<string, FreeformVertex> GetSyntheticChildre(
            [FromQuery] string id,
            [FromQuery] ulong seed = 0
        )
        {
            // Get the GUID.
            if (Guid.TryParse(id, out Guid nodeId))
            {
                RandomInfiniteDirectedGraph data = new RandomInfiniteDirectedGraph(seed: seed, propertyCount: 50, childProbability: 1.0, propertyDensity: 0.10);
                IEnumerable<Edge<FreeformVertex>> edges = data.GetEdges(nodeId);
                IDictionary<string, FreeformVertex> vertices = edges.ToDictionary(
                    edge => edge.Target.Id.ToString(),
                    edge => data.GetProperties(edge.Target.Id)
                );

                return vertices;
            }
            else
                throw new FormatException();
        }

        [HttpGet("hyperthought/{uuid}")]
        public FreeformGraph GetHyperthought(string uuid)
        {
            // Not yet implemented.
            return null;
        }
    }
}