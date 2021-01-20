using System;
using System.Collections.Generic;

using QuikGraph;
using NUnit.Framework;

using CartaCore.Data.Synthetic;

namespace CartaTest
{
    [TestFixture]
    public class UndirectedGraphDatasetTests
    {
        protected readonly int Samples = 25;
        protected readonly int MinVertices = 5;
        protected readonly int MaxVertices = 20;
        protected readonly int MinEdges = 25;
        protected readonly int MaxEdges = 200;

        protected IList<IUndirectedGraph<int, Edge<int>>> Graphs;

        [SetUp]
        public void Setup()
        {
            // Generate the samples we will test.
            UndirectedGraphDataset dataset = new UndirectedGraphDataset(
                Samples,
                MinVertices, MaxVertices,
                MinEdges, MaxEdges
            );
            Graphs = new List<IUndirectedGraph<int, Edge<int>>>(dataset.Generate());
        }

        [Test]
        public void TestNumberSamples()
        {
            Assert.AreEqual(Samples, Graphs.Count);
        }

        [Test]
        public void TestNumberVertices()
        {
            foreach (IUndirectedGraph<int, Edge<int>> graph in Graphs)
            {
                Assert.IsTrue(MinVertices <= graph.VertexCount);
                Assert.IsTrue(MaxVertices >= graph.VertexCount);
            }
        }

        [Test]
        public void TestNumberEdges()
        {
            foreach (IUndirectedGraph<int, Edge<int>> graph in Graphs)
            {
                int vertexCount = graph.VertexCount;
                int edgeCountMax = vertexCount * (vertexCount - 1) / 2;

                Assert.IsTrue(Math.Min(edgeCountMax, MinEdges) <= graph.EdgeCount);
                Assert.IsTrue(MaxEdges >= graph.EdgeCount);
            }
        }

        [Test]
        public void TestNoSelfEdges()
        {
            foreach (IUndirectedGraph<int, Edge<int>> graph in Graphs)
            {
                foreach (Edge<int> edge in graph.Edges)
                {
                    Assert.IsFalse(edge.IsSelfEdge());
                }
            }
        }
    }
}