using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;
using CartaCore.Graphs;
using CartaCore.Graphs.Components;
using CartaWeb.Serialization.Json;

namespace CartaTest.Serialization.Json
{
    /// <summary>
    /// Tests the serialization of freeform graphs into JSON Graph format.
    /// </summary>
    [TestFixture]
    public class JgSerializationTests
    {
        /// <summary>
        /// Tests the serialization and deserialization of a simple undirected graph.
        /// </summary>
        [Test]
        public async Task TestJgUndirected()
        {
            JgFormat sample = await JgFormat.CreateAsync(GraphHelpers.UndirectedGraphSample);

            string str = JsonSerializer.Serialize(sample);
            JgFormat data = JsonSerializer.Deserialize<JgFormat>(str);

            IEnumerableComponent<Vertex, Edge> graph = data.Graph;

            Assert.NotNull(graph);
            Assert.AreEqual(5, await graph.GetVertices().CountAsync());
            Assert.AreEqual(5, await graph.GetEdges().CountAsync());
        }
    }
}