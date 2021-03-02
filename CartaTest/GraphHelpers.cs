using System.Collections.Generic;

using CartaCore.Data;

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
        /// <value>An undirected graph with no properties on the nodes.</value>
        public static FiniteGraph UndirectedGraphSample
        {
            get
            {
                /* Graph Connections
                    0 - 1 - 3
                    0 - 2 - 3
                    3 - 4
                */

                // Setup the graph information.
                FiniteGraph graph = new FiniteGraph(Identity.Create("Undirected w/o Properties"), false);
                Vertex[] vertices = new Vertex[]
                {
                    new Vertex(Identity.Create(0)),
                    new Vertex(Identity.Create(1)),
                    new Vertex(Identity.Create(2)),
                    new Vertex(Identity.Create(3)),
                    new Vertex(Identity.Create(4)),
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
        /// <value>A directed graph with no properties on the nodes.</value>
        public static FiniteGraph DirectedGraphSample
        {
            get
            {
                /* Graph Connections
                    0 -> 1 -> 2 -> 3
                    0 -> 2
                */

                // Setup the graph information.
                FiniteGraph graph = new FiniteGraph(Identity.Create("Directed w/o Properties"), true);
                Vertex[] vertices = new Vertex[]
                {
                    new Vertex(Identity.Create(0)),
                    new Vertex(Identity.Create(1)),
                    new Vertex(Identity.Create(2)),
                    new Vertex(Identity.Create(3)),
                };

                // Construct the vertices and edges of the graph.
                graph.AddVertexRange(vertices);
                graph.AddEdgeRange(new Edge[]
                {
                    new Edge(vertices[0], vertices[1]),
                    new Edge(vertices[0], vertices[2]),
                    new Edge(vertices[1], vertices[2]),
                    new Edge(vertices[2], vertices[3]),
                });

                return graph;
            }
        }

        /// <summary>
        /// Gets an undirected graph sample with properties usable in testing.
        /// </summary>
        /// <value>An undirected graph with properties on the nodes.</value>
        public static FiniteGraph UndirectedPropertyGraphSample
        {
            get
            {
                /* Graph Connections
                    0 - 1 - 2
                */

                // Setup the graph information.
                FiniteGraph graph = new FiniteGraph(Identity.Create("Undirected w/ Properties"), false);
                Vertex[] vertices = new Vertex[]
                {
                    new Vertex
                    (
                        Identity.Create(0),
                        new List<Property>()
                        {
                            new Property
                            (
                                Identity.Create("myNum"),
                                new List<Observation>()
                                {
                                    new Observation { Type = "int", Value = 2 },
                                    new Observation { Type = "int", Value = 5 },
                                }
                            ),
                            new Property
                            (
                                Identity.Create("myFruit"),
                                new List<Observation>()
                                {
                                    new Observation { Type = "string", Value = "orange" }
                                }
                            )
                        }
                    ),
                    new Vertex(Identity.Create(1)),
                    new Vertex
                    (
                        Identity.Create(2),
                        new List<Property>()
                        {
                            new Property
                            (
                                Identity.Create("myShoeSize"),
                                new List<Observation>()
                                {
                                    new Observation { Type = "double", Value = 11.50 }
                                }
                            )
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