using System;
using System.Collections.Generic;

using QuikGraph;

using CartaCore.Data.Freeform;

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
        public static FreeformGraph UndirectedGraphSample
        {
            get
            {
                /* Graph Connections
                    0 - 1 - 3
                    0 - 2 - 3
                    3 - 4
                */

                // Setup the graph information.
                FreeformFiniteGraph graph = new FreeformFiniteGraph(false);
                FreeformVertex[] vertices = new FreeformVertex[]
                {
                    new FreeformVertex(FreeformIdentity.Create(0)),
                    new FreeformVertex(FreeformIdentity.Create(1)),
                    new FreeformVertex(FreeformIdentity.Create(2)),
                    new FreeformVertex(FreeformIdentity.Create(3)),
                    new FreeformVertex(FreeformIdentity.Create(4)),
                };

                // Construct the vertices and edges of the graph.
                graph.AddVertexRange(vertices);
                graph.AddEdgeRange(new FreeformEdge[]
                {
                    new FreeformEdge(vertices[0], vertices[1], 0),
                    new FreeformEdge(vertices[0], vertices[2], 1),
                    new FreeformEdge(vertices[1], vertices[3], 2),
                    new FreeformEdge(vertices[2], vertices[3], 3),
                    new FreeformEdge(vertices[3], vertices[4], 4),
                });

                return graph;
            }
        }

        /// <summary>
        /// Gets a directed graph sample usable in testing.
        /// </summary>
        /// <value>A directed graph with no properties on the nodes.</value>
        public static FreeformGraph DirectedGraphSample
        {
            get
            {
                /* Graph Connections
                    0 -> 1 -> 2 -> 3
                    0 -> 2
                */

                // Setup the graph information.
                FreeformFiniteGraph graph = new FreeformFiniteGraph(true);
                FreeformVertex[] vertices = new FreeformVertex[]
                {
                    new FreeformVertex(FreeformIdentity.Create(0)),
                    new FreeformVertex(FreeformIdentity.Create(1)),
                    new FreeformVertex(FreeformIdentity.Create(2)),
                    new FreeformVertex(FreeformIdentity.Create(3)),
                };

                // Construct the vertices and edges of the graph.
                graph.AddVertexRange(vertices);
                graph.AddEdgeRange(new FreeformEdge[]
                {
                    new FreeformEdge(vertices[0], vertices[1], 0),
                    new FreeformEdge(vertices[0], vertices[2], 1),
                    new FreeformEdge(vertices[1], vertices[2], 0),
                    new FreeformEdge(vertices[2], vertices[3], 0),
                });

                return graph;
            }
        }

        /// <summary>
        /// Gets an undirected graph sample with properties usable in testing.
        /// </summary>
        /// <value>An undirected graph with properties on the nodes.</value>
        public static FreeformGraph UndirectedPropertyGraphSample
        {
            get
            {
                /* Graph Connections
                    0 - 1 - 2
                */

                // Setup the graph information.
                FreeformFiniteGraph graph = new FreeformFiniteGraph(false);
                FreeformVertex[] vertices = new FreeformVertex[]
                {
                    new FreeformVertex(FreeformIdentity.Create(0))
                    {
                        Properties = new List<FreeformProperty>()
                        {
                            new FreeformProperty(FreeformIdentity.Create("myNum"))
                            {
                                Observations = new List<FreeformObservation>()
                                {
                                    new FreeformObservation { Type = "int", Value = 2 },
                                    new FreeformObservation { Type = "int", Value = 5 },
                                }
                            },
                            new FreeformProperty(FreeformIdentity.Create("myFruit"))
                            {
                                Observations = new List<FreeformObservation>()
                                {
                                    new FreeformObservation { Type = "string", Value = "orange" }
                                }
                            }
                        }
                    },
                    new FreeformVertex(FreeformIdentity.Create(1)),
                    new FreeformVertex(FreeformIdentity.Create(2))
                    {
                        Properties = new List<FreeformProperty>()
                        {
                            new FreeformProperty(FreeformIdentity.Create("myShoeSize"))
                            {
                                Observations = new List<FreeformObservation>()
                                {
                                    new FreeformObservation { Type = "double", Value = 11.50 }
                                }
                            }
                        }
                    },
                };

                // Construct the vertices and edges of the graph.
                graph.AddVertexRange(vertices);
                graph.AddEdgeRange(new FreeformEdge[]
                {
                    new FreeformEdge(vertices[0], vertices[1], 0),
                    new FreeformEdge(vertices[1], vertices[2], 1)
                });

                return graph;
            }
        }
    }
}