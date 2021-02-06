using System.Text.Json;

using QuikGraph;
using NUnit.Framework;

using CartaCore.Data;
using CartaWeb.Serialization.Json;

namespace CartaTest.Serialization.Json
{
    using FreeformGraph = IMutableVertexAndEdgeSet<FreeformVertex, FreeformEdge>;

    /// <summary>
    /// Tests the serialization of freeform graphs into Vis format.
    /// </summary>
    [TestFixture]
    public class VisSerializationTests
    {
        /// <summary>
        /// Tests the serialization and deserialization of a simple undirected graph.
        /// </summary>
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