using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using QuikGraph;
using NUnit.Framework;

using CartaCore.Data;
using CartaWeb.Serialization.Json;

namespace CartaTest.Serialization.Json
{
    using FreeformGraph = IMutableVertexAndEdgeSet<FreeformVertex, FreeformEdge>;

    [TestFixture]
    public class JgfSerializationTests
    {
        protected FreeformGraph TestGraph;

        [SetUp]
        public void SetUp()
        {
            // Create our test graph.
            IList<FreeformVertex> vertices = new List<FreeformVertex>
            {
                new FreeformVertex { Id = Guid.NewGuid() },
                new FreeformVertex { Id = Guid.NewGuid() },
                new FreeformVertex { Id = Guid.NewGuid() },
                new FreeformVertex { Id = Guid.NewGuid() },
                new FreeformVertex { Id = Guid.NewGuid() }
            };
            IList<FreeformEdge> edges = new FreeformEdge[] {
                new FreeformEdge(vertices[0], vertices[1]),
                new FreeformEdge(vertices[0], vertices[2]),
                new FreeformEdge(vertices[1], vertices[3]),
                new FreeformEdge(vertices[2], vertices[3]),
                new FreeformEdge(vertices[3], vertices[4])
            };

            AdjacencyGraph<FreeformVertex, FreeformEdge> graph = new AdjacencyGraph<FreeformVertex, FreeformEdge>();
            graph.AddVertexRange(vertices);
            graph.AddEdgeRange(edges);
            TestGraph = graph;
        }

        [Test]
        public void TestJgfSerialize()
        {
            JgFormat graph = new JgFormat(TestGraph);
            string str = JsonSerializer.Serialize<JgFormat>(graph);

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
                        ""00000000-0000-4000-8000-000000000001"": {},
                        ""00000000-0000-4000-8000-000000000002"": {}
                    },
                    ""edges"": [
                        {
                            ""source"": ""00000000-0000-4000-8000-000000000001"",
                            ""target"": ""00000000-0000-4000-8000-000000000002""
                        }
                    ]
                }
            }
            ";

            JgFormat jgf = JsonSerializer.Deserialize<JgFormat>(jgfString);
            FreeformGraph graph = jgf.Graph;

            Assert.AreEqual(2, graph.VertexCount);
            Assert.AreEqual(1, graph.EdgeCount);
            foreach (Guid id in new Guid[]
                {
                    Guid.Parse("00000000-0000-4000-8000-000000000001"),
                    Guid.Parse("00000000-0000-4000-8000-000000000002")
                }
            )
            {
                Assert.AreEqual(
                    1,
                    graph.Vertices
                        .Where(vertex => vertex.Id == id)
                        .Count()
                );
            }
            foreach (Edge<FreeformVertex> edge in new Edge<FreeformVertex>[]
                {
                    new Edge<FreeformVertex>(
                        new FreeformVertex { Id = Guid.Parse("00000000-0000-4000-8000-000000000001") },
                        new FreeformVertex { Id = Guid.Parse("00000000-0000-4000-8000-000000000002") }
                    )
                }
            )
            {
                Assert.AreEqual(
                    1,
                    graph.Edges
                        .Where(
                            other =>
                                edge.Source.Id == other.Source.Id &&
                                edge.Target.Id == other.Target.Id
                        )
                        .Count()
                );
            }
        }

        [Test]
        public void TestJgfReserialize()
        {
            string jgfString;
            FreeformGraph jgfGraph;

            JgFormat graph = new JgFormat(TestGraph);
            jgfString = JsonSerializer.Serialize<JgFormat>(graph);
            jgfGraph = (JsonSerializer.Deserialize<JgFormat>(jgfString)).Graph;

            Assert.AreEqual(TestGraph.VertexCount, jgfGraph.VertexCount);
            Assert.AreEqual(TestGraph.EdgeCount, jgfGraph.EdgeCount);
        }
    }
}