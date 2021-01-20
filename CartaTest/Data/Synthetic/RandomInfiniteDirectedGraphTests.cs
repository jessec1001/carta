using System;
using System.Collections.Generic;
using System.Linq;

using QuikGraph;
using NUnit.Framework;

using CartaCore.Data;
using CartaCore.Data.Synthetic;

namespace CartaTest.Data.Synthetic
{
    [TestFixture]
    public class RandomInfiniteDirectedGraphTests
    {
        protected RandomInfiniteDirectedGraph Graph;

        [SetUp]
        public void Setup()
        {
            Graph = new RandomInfiniteDirectedGraph(
                propertyCount: 10
            );
        }

        [Test]
        public void TestVertexGeneration()
        {
            Guid id = Guid.NewGuid();

            FreeformVertex vertex = Graph.GetVertexProperties(id);
            IList<Edge<FreeformVertex>> edges = Graph.GetVertexEdges(id).ToList();

            Assert.Pass();
        }

        [Test]
        public void TestDeterministic()
        {
            Guid id = Guid.NewGuid();

            FreeformVertex vertex1 = Graph.GetVertexProperties(id);
            IList<Edge<FreeformVertex>> edges1 = Graph.GetVertexEdges(id).ToList();

            FreeformVertex vertex2 = Graph.GetVertexProperties(id);
            IList<Edge<FreeformVertex>> edges2 = Graph.GetVertexEdges(id).ToList();

            Assert.AreEqual(vertex1.Id, vertex2.Id);
            Assert.AreEqual(vertex1.Properties.Count, vertex2.Properties.Count);
            for (int k = 0; k < vertex1.Properties.Count; k++)
            {
                Assert.AreEqual(vertex1.Properties.Keys[k], vertex2.Properties.Keys[k]);
                Assert.AreEqual(vertex1.Properties.Values[k].Type, vertex2.Properties.Values[k].Type);
                Assert.AreEqual(vertex1.Properties.Values[k].Value, vertex2.Properties.Values[k].Value);
            }

            Assert.AreEqual(edges1.Count, edges2.Count);
            for (int k = 0; k < edges1.Count; k++)
            {
                Assert.AreEqual(edges1[k].Source, edges2[k].Source);
                Assert.AreEqual(edges1[k].Target, edges2[k].Target);
            }
        }
    }
}