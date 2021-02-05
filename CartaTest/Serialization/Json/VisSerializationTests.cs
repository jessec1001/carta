using System.Text.Json;

using QuikGraph;
using NUnit.Framework;

using CartaCore.Data;
using CartaWeb.Serialization.Json;

namespace CartaTest.Serialization.Json
{
    using FreeformGraph = IMutableVertexAndEdgeSet<FreeformVertex, FreeformEdge>;

    [TestFixture]
    public class VisSerializationTests
    {
        [Test]
        public void TestVisUndirected()
        {
            VisFormat sample = new VisFormat(Helpers.UndirectedGraphSample);

            string str = JsonSerializer.Serialize<VisFormat>(sample);
            VisFormat data = JsonSerializer.Deserialize<VisFormat>(str);

            FreeformGraph graph = data.Graph;

            Assert.NotNull(graph);
            Assert.AreEqual(5, graph.VertexCount);
            Assert.AreEqual(5, graph.EdgeCount);
        }
    }
}