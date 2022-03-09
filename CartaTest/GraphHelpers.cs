using System.Collections.Generic;
using CartaCore.Graphs;

namespace CartaTest
{
    /// <summary>
    /// A class of helpers for executing graph serialization tests.
    /// </summary>
    public static class GraphHelpers
    {
        /// <summary>
        /// Gets an undirected graph sample usable in testing.
        /// </summary>
        /// <value>An undirected graph with no properties on the vertices.</value>
        public static MemoryGraph UndirectedGraphSample
        {
            get
            {
                /* Graph Connections
                    0 - 1 - 3
                    0 - 2 - 3
                    3 - 4
                */

                // Setup the graph information.
                MemoryGraph graph = new("Undirected w/o Properties");
                Vertex[] vertices = new Vertex[]
                {
                    new Vertex("0"),
                    new Vertex("1"),
                    new Vertex("2"),
                    new Vertex("3"),
                    new Vertex("4"),
                };

                // Construct the vertices and edges of the graph.
                graph.AddVertexRange(vertices);
                graph.AddEdgeRange(new Edge[]
                {
                    new Edge(vertices[0], vertices[1]),
                    new Edge(vertices[0], vertices[2]),
                    new Edge(vertices[1], vertices[3]),
                    new Edge(vertices[2], vertices[3]),
                    new Edge(vertices[3], vertices[4]),
                });

                return graph;
            }
        }

        /// <summary>
        /// Gets a directed graph sample usable in testing.
        /// </summary>
        /// <value>A directed graph with no properties on the vertices.</value>
        public static MemoryGraph DirectedGraphSample
        {
            get
            {
                /* Graph Connections
                    0 -> 1 -> 2 -> 3
                    0 -> 2
                */

                // Setup the graph information.
                MemoryGraph graph = new("Directed w/o Properties");
                Vertex[] vertices = new Vertex[]
                {
                    new Vertex("0"),
                    new Vertex("1"),
                    new Vertex("2"),
                    new Vertex("3"),
                };

                // Construct the vertices and edges of the graph.
                graph.AddVertexRange(vertices);
                graph.AddEdgeRange(new Edge[]
                {
                    new Edge(vertices[0], vertices[1]) { Directed = true },
                    new Edge(vertices[0], vertices[2]) { Directed = true },
                    new Edge(vertices[1], vertices[2]) { Directed = true },
                    new Edge(vertices[2], vertices[3]) { Directed = true },
                });

                return graph;
            }
        }

        /// <summary>
        /// Gets an undirected graph sample with properties usable in testing.
        /// </summary>
        /// <value>An undirected graph with properties on the vertices.</value>
        public static MemoryGraph UndirectedPropertyGraphSample
        {
            get
            {
                /* Graph Connections
                    0 - 1 - 2
                */

                // Setup the graph information.
                MemoryGraph graph = new("Undirected w/ Properties");
                Vertex[] vertices = new Vertex[]
                {
                    new Vertex
                    (
                        "0",
                        new Dictionary<string, IProperty>
                        {
                            ["myNum"] = new Property(new[] { 2, 5 }),
                            ["myFruit"] = new Property("orange"),
                        }
                    ),
                    new Vertex("1"),
                    new Vertex
                    (
                        "2",
                        new Dictionary<string, IProperty>
                        {
                            ["myShoeSize"] = new Property(11.50)
                        }
                    )
                };

                // Construct the vertices and edges of the graph.
                graph.AddVertexRange(vertices);
                graph.AddEdgeRange(new Edge[]
                {
                    new Edge(vertices[0], vertices[1]),
                    new Edge(vertices[1], vertices[2])
                });

                return graph;
            }
        }
    }
}