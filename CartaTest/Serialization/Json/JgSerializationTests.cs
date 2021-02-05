using System.Text.Json;

using QuikGraph;
using NUnit.Framework;

using CartaCore.Data;
using CartaWeb.Serialization.Json;

namespace CartaTest.Serialization.Json
{
    using FreeformGraph = IMutableVertexAndEdgeSet<FreeformVertex, FreeformEdge>;

    [TestFixture]
    public class JgfSerializationTests
    {
        [Test]
        public void TestJgUndirected()
        {
            JgFormat sample = new JgFormat(Helpers.UndirectedGraphSample);

            string str = JsonSerializer.Serialize<JgFormat>(sample);
            JgFormat data = JsonSerializer.Deserialize<JgFormat>(str);

            FreeformGraph graph = data.Graph;

            Assert.NotNull(graph);
            Assert.AreEqual(5, graph.VertexCount);
            Assert.AreEqual(5, graph.EdgeCount);
        }
    }
}