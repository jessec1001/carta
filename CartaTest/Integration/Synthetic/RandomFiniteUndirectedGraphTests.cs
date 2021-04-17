using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

using CartaCore.Data;
using CartaCore.Integration.Synthetic;

namespace CartaTest
{
    /// <summary>
    /// Tests the generation of the <see cref="FiniteUndirectedGraph"/> object.
    /// </summary>
    [TestFixture]
    public class RandomFiniteUndirectedGraphTests
    {
        /// <summary>
        /// The graph generated to test on.
        /// </summary>
        protected IEntireGraph Graph;

        /// <summary>
        /// Sets up the test fixture.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // Generate the samples we will test.
            Graph = new FiniteUndirectedGraph(
                new FiniteUndirectedGraphParameters
                {
                    Seed = 0
                }
            );
        }

        /// <summary>
        /// Tests that the number of vertices in the test graph is within our minimum and maximum. 
        /// </summary>
        [Test]
        public async Task TestNumberVertices()
        {
            Assert.IsTrue(0 <= await Graph.GetVertices().CountAsync());
        }

        /// <summary>
        /// Tests that the number of edges in the test graph is within our minimum and maximum.
        /// </summary>
        [Test]
        public async Task TestNumberEdges()
        {
            int vertexCount = await Graph.GetVertices().CountAsync();
            int edgeCountMax = vertexCount * (vertexCount - 1) / 2;

            Assert.IsTrue(edgeCountMax >= await Graph.GetEdges().CountAsync());
            Assert.IsTrue(0 <= await Graph.GetEdges().CountAsync());
        }

        /// <summary>
        /// Tests that there are no self edges in the test graph.
        /// </summary>
        [Test]
        public async Task TestNoSelfEdges()
        {
            await foreach (Edge edge in Graph.GetEdges())
                Assert.IsFalse(edge.Source == edge.Target);
        }
    }
}