using System;

using QuikGraph;
using NUnit.Framework;

using CartaCore.Data;
using CartaCore.Data.Synthetic;

namespace CartaTest
{
    using FreeformGraph = IMutableVertexAndEdgeSet<FreeformVertex, FreeformEdge>;

    /// <summary>
    /// Tests the generation of the <see cref="RandomFiniteUndirectedGraph"/> object.
    /// </summary>
    [TestFixture]
    public class RandomFiniteUndirectedGraphTests
    {
        /// <summary>
        /// The graph generated to test on.
        /// </summary>
        protected FreeformGraph TestGraph;

        /// <summary>
        /// Sets up the test fixture.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // Generate the samples we will test.
            ISampledGraph graph = new RandomFiniteUndirectedGraph(
                new RandomFiniteUndirectedGraphOptions
                {
                    Seed = 0
                }
            );
            TestGraph = graph.GetEntire();
        }

        /// <summary>
        /// Tests that the number of vertices in the test graph is within our minimum and maximum. 
        /// </summary>
        [Test]
        public void TestNumberVertices()
        {
            Assert.IsTrue(0 <= TestGraph.VertexCount);
        }

        /// <summary>
        /// Tests that the number of edges in the test graph is within our minimum and maximum.
        /// </summary>
        [Test]
        public void TestNumberEdges()
        {
            int vertexCount = TestGraph.VertexCount;
            int edgeCountMax = vertexCount * (vertexCount - 1) / 2;

            Assert.IsTrue(edgeCountMax >= TestGraph.EdgeCount);
            Assert.IsTrue(0 <= TestGraph.EdgeCount);
        }

        /// <summary>
        /// Tests that there are no self edges in the test graph.
        /// </summary>
        [Test]
        public void TestNoSelfEdges()
        {
            foreach (Edge<FreeformVertex> edge in TestGraph.Edges)
            {
                Assert.IsFalse(edge.IsSelfEdge());
            }
        }
    }
}