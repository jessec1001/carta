using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using QuikGraph;

using CartaCore.Data.Synthetic;
using CartaCore.Serialization.Json.Jgf;

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

        [HttpGet]
        [Produces("text/json")]
        public Jgf Get()
        {
            // Generate and return graph.
            UndirectedGraphDataset dataset = new UndirectedGraphDataset(
                1,
                minVertices: 20, maxVertices: 100,
                minEdges: 0, maxEdges: 100
            );
            IUndirectedGraph<int, Edge<int>> graph = dataset.Generate().First();

            return new Jgf(graph);
        }
    }
}