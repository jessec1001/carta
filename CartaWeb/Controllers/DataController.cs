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
        public FreeformGraph GetSynthetic()
        {
            // Generate and return graph.
            RandomFiniteUndirectedGraph dataset = new RandomFiniteUndirectedGraph(
                1,
                minVertices: 20, maxVertices: 100,
                minEdges: 20, maxEdges: 100
            );
            FreeformGraph graph = dataset.GetGraph();

            return graph;
        }

        [HttpGet("hyperthought/{uuid}")]
        public FreeformGraph GetHyperthought(string uuid)
        {
            // Not yet implemented.
            return null;
        }
    }
}