using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using QuikGraph;

using CartaCore.Data.Synthetic;
using CartaCore.Serialization.Xml;

namespace CartaWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GraphController : ControllerBase
    {
        private readonly ILogger<GraphController> _logger;

        public GraphController(ILogger<GraphController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Produces("text/xml")]
        public Gexf Get()
        {
            // Generate adn return graph.
            UndirectedGraphDataset dataset = new UndirectedGraphDataset(
                1,
                minVertices: 20, maxVertices: 100,
                minEdges: 0, maxEdges: 100
            );
            IUndirectedGraph<int, Edge<int>> graph = dataset.Generate().First();

            return new Gexf(graph);
        }
    }
}