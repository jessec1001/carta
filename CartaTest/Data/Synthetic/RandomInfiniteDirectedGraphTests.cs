using System;
using System.Collections.Generic;
using System.Linq;

using QuikGraph;
using NUnit.Framework;

using CartaCore.Data;
using CartaCore.Data.Synthetic;

namespace CartaTest.Data.Synthetic
{
    /// <summary>
    /// Tests the generation of the <see cref="RandomInfiniteDirectedGraph"/> object.
    /// </summary>
    [TestFixture]
    public class RandomInfiniteDirectedGraphTests
    {
        /// <summary>
        /// The graph generated to test on.
        /// </summary>
        protected RandomInfiniteDirectedGraph Graph;

        /// <summary>
        /// Sets up the test fixture.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            Graph = new RandomInfiniteDirectedGraph(
                new RandomInfiniteDirectedGraphOptions
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
            FreeformVertex vertex = Graph.GetProperties(id);
            IList<Edge<FreeformVertex>> edges = Graph.GetEdges(id).ToList();

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
            FreeformVertex vertex1 = Graph.GetProperties(id);
            IList<Edge<FreeformVertex>> edges1 = Graph.GetEdges(id).ToList();

            FreeformVertex vertex2 = Graph.GetProperties(id);
            IList<Edge<FreeformVertex>> edges2 = Graph.GetEdges(id).ToList();

            // Check that the vertex properties are the same.
            Assert.AreEqual(vertex1.Id, vertex2.Id);
            Assert.AreEqual(vertex1.Properties.Count, vertex2.Properties.Count);
            for (int k = 0; k < vertex1.Properties.Count; k++)
            {
                Assert.AreEqual(vertex1.Properties.Keys[k], vertex2.Properties.Keys[k]);
                Assert.AreEqual(vertex1.Properties.Values[k].Type, vertex2.Properties.Values[k].Type);
                Assert.AreEqual(vertex1.Properties.Values[k].Value, vertex2.Properties.Values[k].Value);
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