using System.Text.Json;

using NUnit.Framework;

using CartaCore.Data.Freeform;
using CartaWeb.Serialization.Json;

namespace CartaTest.Serialization.Json
{
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
            VisFormat sample = new VisFormat(GraphHelpers.UndirectedGraphSample);

            string str = JsonSerializer.Serialize<VisFormat>(sample);
            VisFormat data = JsonSerializer.Deserialize<VisFormat>(str);

            FreeformGraph graph = data.Graph;

            Assert.NotNull(graph);
            Assert.AreEqual(5, graph.VertexCount);
            Assert.AreEqual(5, graph.EdgeCount);
            Assert.IsTrue(graph.ContainsVertex(new FreeformVertex(FreeformIdentity.Create(0))));
            Assert.IsTrue(graph.ContainsVertex(new FreeformVertex(FreeformIdentity.Create(4))));
            Assert.IsFalse(graph.ContainsVertex(new FreeformVertex(FreeformIdentity.Create(5))));
            Assert.IsTrue(graph.ContainsEdge(new FreeformEdge
            (
                FreeformIdentity.Create(1),
                FreeformIdentity.Create(3),
                FreeformIdentity.Create("1.2")
            )));
            Assert.IsTrue(graph.ContainsEdge(new FreeformEdge
            (
                FreeformIdentity.Create(2),
                FreeformIdentity.Create(3),
                FreeformIdentity.Create("2.3")
            )));
            Assert.IsTrue(graph.ContainsEdge(new FreeformEdge
            (
                FreeformIdentity.Create(0),
                FreeformIdentity.Create(1),
                FreeformIdentity.Create("0.0")
            )));
            Assert.IsFalse(graph.ContainsEdge(new FreeformEdge
            (
                FreeformIdentity.Create(0),
                FreeformIdentity.Create(1),
                FreeformIdentity.Create("0.3")
            )));
        }
    }
}