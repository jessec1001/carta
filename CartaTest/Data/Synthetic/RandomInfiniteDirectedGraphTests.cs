using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using CartaCore.Data.Freeform;
using CartaCore.Data.Synthetic;

namespace CartaTest.Data.Synthetic
{
    /// <summary>
    /// Tests the generation of the <see cref="InfiniteDirectedGraph"/> object.
    /// </summary>
    [TestFixture]
    public class RandomInfiniteDirectedGraphTests
    {
        /// <summary>
        /// The graph generated to test on.
        /// </summary>
        protected FreeformDynamicGraph<Guid> Graph;

        /// <summary>
        /// Sets up the test fixture.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            Graph = new InfiniteDirectedGraph(
                new InfiniteDirectedGraphParameters
                {
                    PropertyCount = 10
                }
            );
        }

        /// <summary>
        /// Tests that we can generate a random vertex without error.
        /// </summary>
        [Test]
        public void TestVertexGeneration()
        {
            // Get a random vertex ID.
            Guid id = Guid.NewGuid();

            // Generate the properties and edges of the vertex.
            FreeformVertex vertex = Graph.GetVertex(id);
            IList<FreeformEdge> edges = Graph.GetEdges(id).ToList();

            Assert.Pass();
        }

        /// <summary>
        /// Tests that a specific randomly generated vertex will always deterministically return the same properties and
        /// edges. 
        /// </summary>
        [Test]
        public void TestDeterministic()
        {
            // Get a random vertex ID.
            Guid id = Guid.NewGuid();

            // Generate the two instances of the vertex properties and edges.
            FreeformVertex vertex1 = Graph.GetVertex(id);
            IList<FreeformEdge> edges1 = Graph.GetEdges(id).ToList();

            FreeformVertex vertex2 = Graph.GetVertex(id);
            IList<FreeformEdge> edges2 = Graph.GetEdges(id).ToList();

            // Check that the vertex properties are the same.
            IList<FreeformProperty> Properties1 = vertex1.Properties.ToList();
            IList<FreeformProperty> Properties2 = vertex2.Properties.ToList();
            Assert.AreEqual(vertex1.Identifier, vertex2.Identifier);
            Assert.AreEqual(Properties1.Count, Properties2.Count);
            for (int k = 0; k < Properties1.Count; k++)
            {
                IList<FreeformObservation> Observations1 = Properties1[k].Observations.ToList();
                IList<FreeformObservation> Observations2 = Properties2[k].Observations.ToList();
                Assert.AreEqual(Properties1[k].Identifier, Properties2[k].Identifier);
                Assert.AreEqual(Observations1.Count, Observations2.Count);
                for (int l = 0; l < Observations1.Count; l++)
                {
                    Assert.AreEqual(Observations1[l].Type, Observations2[l].Type);
                    Assert.AreEqual(Observations1[l].Value, Observations2[l].Value);
                }
            }

            // Check that the vertex edges are the same.
            Assert.AreEqual(edges1.Count, edges2.Count);
            for (int k = 0; k < edges1.Count; k++)
            {
                Assert.AreEqual(edges1[k].Source, edges2[k].Source);
                Assert.AreEqual(edges1[k].Target, edges2[k].Target);
            }
        }
    }
}