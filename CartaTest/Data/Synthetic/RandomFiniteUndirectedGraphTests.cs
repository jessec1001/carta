using System;

using QuikGraph;
using NUnit.Framework;

using CartaCore.Data.Freeform;
using CartaCore.Data.Synthetic;

namespace CartaTest
{
    /// <summary>
    /// Tests the generation of the <see cref="FiniteUndirectedGraph"/> object.
    /// </summary>
    [TestFixture]
    public class RandomFiniteUndirectedGraphTests
    {
        /// <summary>
        /// The minimum (inclusive) number of vertices.
        /// </summary>
        protected readonly int MinVertices = 5;
        /// <summary>
        /// The maximum (inclusive) number of vertices.
        /// </summary>
        protected readonly int MaxVertices = 20;
        /// <summary>
        /// The minimum (inclusive) number of edges.
        /// </summary>
        protected readonly int MinEdges = 25;
        /// <summary>
        /// The maxmimum (inclusive) number of edges.
        /// </summary>
        protected readonly int MaxEdges = 200;

        /// <summary>
        /// The graph generated to test on.
        /// </summary>
        protected FreeformGraph Graph;

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
                    Seed = 0,
                    MinVertices = MinVertices,
                    MaxVertices = MaxVertices,
                    MinEdges = MinEdges,
                    MaxEdges = MaxEdges
                }
            );
        }

        /// <summary>
        /// Tests that the number of vertices in the test graph is within our minimum and maximum. 
        /// </summary>
        [Test]
        public void TestNumberVertices()
        {
            Assert.IsTrue(MinVertices <= Graph.VertexCount);
            Assert.IsTrue(MaxVertices >= Graph.VertexCount);
        }

        /// <summary>
        /// Tests that the number of edges in the test graph is within our minimum and maximum.
        /// </summary>
        [Test]
        public void TestNumberEdges()
        {
            int vertexCount = Graph.VertexCount;
            int edgeCountMax = vertexCount * (vertexCount - 1) / 2;

            Assert.IsTrue(Math.Min(edgeCountMax, MinEdges) <= Graph.EdgeCount);
            Assert.IsTrue(MaxEdges >= Graph.EdgeCount);
        }

        /// <summary>
        /// Tests that there are no self edges in the test graph.
        /// </summary>
        [Test]
        public void TestNoSelfEdges()
        {
            foreach (FreeformEdge edge in Graph.Edges)
            {
                Assert.IsFalse(edge.IsSelfEdge());
            }
        }
    }
}