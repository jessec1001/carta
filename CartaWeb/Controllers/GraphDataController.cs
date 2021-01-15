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
    [Route("[controller]")]
    public class GraphDataController : ControllerBase
    {
        private readonly ILogger<GraphDataController> _logger;

        public GraphDataController(ILogger<GraphDataController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Produces("text/xml")]
        public ContentResult Get()
        {
            // Generate graph.
            UndirectedGraphDataset dataset = new UndirectedGraphDataset(
                1,
                minVertices: 20, maxVertices: 100,
                minEdges: 0, maxEdges: 100
            );
            IUndirectedGraph<int, Edge<int>> graph = dataset.Generate().First();

            // Serialize graph.
            string xml;
            using (StringWriter sw = new StringWriter())
            {
                using (XmlWriter xw = XmlWriter.Create(sw))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Gexf));
                    serializer.Serialize(xw, new Gexf(graph));
                }
                xml = sw.ToString();
            }

            // Return graph in GEXF format.
            return new ContentResult
            {
                Content = xml,
                ContentType = "text/xml",
                StatusCode = 200
            };
        }
    }
}