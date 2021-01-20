using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using QuikGraph;

using CartaCore.Data.Synthetic;

namespace CartaWeb.Controllers
{
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
        public IUndirectedGraph<int, Edge<int>> GetSynthetic()
        {
            // Generate and return graph.
            UndirectedGraphDataset dataset = new UndirectedGraphDataset(
                1,
                minVertices: 20, maxVertices: 100,
                minEdges: 0, maxEdges: 100
            );
            IUndirectedGraph<int, Edge<int>> graph = dataset.Generate().First();

            return graph;
        }

        [HttpGet("hyperthought/{uuid}")]
        public IUndirectedGraph<int, Edge<int>> GetHyperthought(string uuid)
        {
            // Not yet implemented.
            return null;
        }
    }
}