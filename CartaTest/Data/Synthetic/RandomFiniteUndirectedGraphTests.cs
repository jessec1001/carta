using System;

using QuikGraph;
using NUnit.Framework;

using CartaCore.Data;
using CartaCore.Data.Synthetic;

namespace CartaTest
{
    using FreeformGraph = IEdgeListAndIncidenceGraph<FreeformVertex, Edge<FreeformVertex>>;

    [TestFixture]
    public class RandomFiniteUndirectedGraphTests
    {
        protected readonly int MinVertices = 5;
        protected readonly int MaxVertices = 20;
        protected readonly int MinEdges = 25;
        protected readonly int MaxEdges = 200;

        protected FreeformGraph TestGraph;

        [SetUp]
        public void Setup()
        {
            // Generate the samples we will test.
            RandomFiniteUndirectedGraph graph = new RandomFiniteUndirectedGraph(
                seed: 0,
                minVertices: MinVertices, maxVertices: MaxVertices,
                minEdges: MinEdges, maxEdges: MaxEdges
            );
            TestGraph = graph.GetGraph();
        }

        [Test]
        public void TestNumberVertices()
        {
            Assert.IsTrue(MinVertices <= TestGraph.VertexCount);
            Assert.IsTrue(MaxVertices >= TestGraph.VertexCount);
        }

        [Test]
        public void TestNumberEdges()
        {
            int vertexCount = TestGraph.VertexCount;
            int edgeCountMax = vertexCount * (vertexCount - 1) / 2;

            Assert.IsTrue(Math.Min(edgeCountMax, MinEdges) <= TestGraph.EdgeCount);
            Assert.IsTrue(MaxEdges >= TestGraph.EdgeCount);
        }

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