using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using CartaCore.Serialization.Json.Jgf;
using QuikGraph;
using NUnit.Framework;

namespace CartaTest.Serialization.Json
{
    [TestFixture]
    public class JgfSerializationTests
    {
        protected IUndirectedGraph<int, Edge<int>> TestGraph;

        [SetUp]
        public void SetUp()
        {
            // Create our test graph.
            IList<Edge<int>> edgeList = new Edge<int>[] {
                new Edge<int>(0, 1),
                new Edge<int>(0, 2),
                new Edge<int>(1, 3),
                new Edge<int>(2, 3),
                new Edge<int>(3, 4)
            };
            TestGraph = edgeList.ToUndirectedGraph<int, Edge<int>>();
        }

        [Test]
        public void TestJgfSerialize()
        {
            Jgf graph = new Jgf(TestGraph);
            string str = JsonSerializer.Serialize<Jgf>(graph);

            Assert.Pass();
        }

        [Test]
        public void TestJgfDeserialize()
        {
            string jgfString =
            @"
            {
                ""graph"": {
                    ""nodes"": {
                        ""1"": {},
                        ""2"": {}
                    },
                    ""edges"": [
                        {
                            ""source"": ""1"",
                            ""target"": ""2""
                        }
                    ]
                }
            }
            ";

            IUndirectedGraph<int, Edge<int>> graph;
            Jgf jgf = JsonSerializer.Deserialize<Jgf>(jgfString);
            graph = jgf.GraphValue;

            Assert.AreEqual(2, graph.VertexCount);
            Assert.AreEqual(1, graph.EdgeCount);
            foreach (int vertex in new int[] { 1, 2 })
                Assert.IsTrue(
                    graph.Vertices.Contains(vertex)
                );
            foreach (Edge<int> edge in new Edge<int>[] { new Edge<int>(1, 2) })
                Assert.AreEqual(1,
                    graph.Edges
                    .Where(
                        other =>
                            edge.Source == other.Source &&
                            edge.Target == other.Target
                    )
                    .Count()
                );
        }

        [Test]
        public void TestJgfReserialize()
        {
            string jgfString;
            IUndirectedGraph<int, Edge<int>> jgfGraph;

            Jgf graph = new Jgf(TestGraph);
            jgfString = JsonSerializer.Serialize<Jgf>(graph);
            jgfGraph = (JsonSerializer.Deserialize<Jgf>(jgfString)).GraphValue;

            Assert.AreEqual(TestGraph.VertexCount, jgfGraph.VertexCount);
            Assert.AreEqual(TestGraph.EdgeCount, jgfGraph.EdgeCount);
        }
    }
}