using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using NUnit.Framework;

using CartaCore.Data;
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
        public async Task TestVisUndirected()
        {
            VisFormat sample = await VisFormat.CreateAsync(GraphHelpers.UndirectedGraphSample);

            string str = JsonSerializer.Serialize<VisFormat>(sample);
            VisFormat data = JsonSerializer.Deserialize<VisFormat>(str);

            IEntireGraph graph = data.Graph;

            Assert.NotNull(graph);
            Assert.AreEqual(5, await graph.GetVertices().CountAsync());
            Assert.AreEqual(5, await graph.GetEdges().CountAsync());
        }
    }
}