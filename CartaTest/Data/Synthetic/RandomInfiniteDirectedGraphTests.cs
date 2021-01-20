using System;
using System.Collections.Generic;
using System.Linq;

using QuikGraph;
using NUnit.Framework;

using CartaCore.Utility;
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
    }
}