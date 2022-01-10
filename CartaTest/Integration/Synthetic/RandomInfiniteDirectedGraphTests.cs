using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

using CartaCore.Data;
using CartaCore.Integration.Synthetic;

namespace CartaTest.Integration.Synthetic
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
        protected IDynamicOutGraph<Vertex> Graph;

        /// <summary>
        /// Sets up the test fixture.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            Graph = new InfiniteDirectedGraph(new InfiniteDirectedGraphParameters());
        }

        /// <summary>
        /// Tests that we can generate a random vertex without error.
        /// </summary>
        [Test]
        public async Task TestVertexGeneration()
        {
            // Get a random vertex ID.
            Identity id = Identity.Create(Guid.NewGuid());

            // Generate the properties and edges of the vertex.
            Vertex vertex = await Graph.GetVertex(id);

            Assert.IsNotNull(vertex.OutEdges);
            Assert.Pass();
        }

        /// <summary>
        /// Tests that a specific randomly generated vertex will always deterministically return the same properties and
        /// edges. 
        /// </summary>
        [Test]
        public async Task TestDeterministic()
        {
            // Get a random vertex ID.
            Identity id = Identity.Create(Guid.NewGuid());

            // Generate the two instances of the vertex properties and edges.
            Vertex vertex1 = await Graph.GetVertex(id);
            Vertex vertex2 = await Graph.GetVertex(id);

            // Check that the vertex properties are the same.
            IList<Property> properties1 = vertex1.Properties.ToList();
            IList<Property> properties2 = vertex2.Properties.ToList();
            Assert.AreEqual(vertex1.Identifier, vertex2.Identifier);
            Assert.AreEqual(properties1.Count, properties2.Count);
            for (int k = 0; k < properties1.Count; k++)
            {
                object value1 = properties1[k].Value;
                object value2 = properties2[k].Value;
                Assert.AreEqual(properties1[k].Identifier, properties2[k].Identifier);
                Assert.AreEqual(value1, value2);
            }

            // Check that the vertex edges are the same.
            IList<Edge> edges1 = vertex1.OutEdges.ToList();
            IList<Edge> edges2 = vertex2.OutEdges.ToList();
            Assert.AreEqual(edges1.Count, edges2.Count);
            for (int k = 0; k < edges1.Count; k++)
            {
                Assert.AreEqual(edges1[k].Source, edges2[k].Source);
                Assert.AreEqual(edges1[k].Target, edges2[k].Target);
            }
        }
    }
}