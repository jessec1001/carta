using System;
using System.Collections.Generic;

using QuikGraph;

using CartaCore.Data;

namespace CartaTest.Serialization
{
    using FreeformGraph = IMutableVertexAndEdgeSet<FreeformVertex, FreeformEdge>;

    /// <summary>
    /// A class of helpers for executing graph serialization tests.
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Gets an undirected graph sample usable in testing.
        /// </summary>
        /// <value>An undirected graph with no properties on the nodes.</value>
        public static FreeformGraph UndirectedGraphSample
        {
            get
            {
                // Setup the graph information.
                FreeformGraph graph = new UndirectedGraph<FreeformVertex, FreeformEdge>();
                FreeformVertex[] vertices = new FreeformVertex[]
                {
                    new FreeformVertex(Guid.NewGuid()) { Properties = new SortedList<string, FreeformProperty>() },
                    new FreeformVertex(Guid.NewGuid()) { Properties = new SortedList<string, FreeformProperty>() },
                    new FreeformVertex(Guid.NewGuid()) { Properties = new SortedList<string, FreeformProperty>() },
                    new FreeformVertex(Guid.NewGuid()) { Properties = new SortedList<string, FreeformProperty>() },
                    new FreeformVertex(Guid.NewGuid()) { Properties = new SortedList<string, FreeformProperty>() },
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
                // Setup the graph information.
                FreeformGraph graph = new AdjacencyGraph<FreeformVertex, FreeformEdge>();
                FreeformVertex[] vertices = new FreeformVertex[]
                {
                    new FreeformVertex(Guid.NewGuid()) { Properties = new SortedList<string, FreeformProperty>() },
                    new FreeformVertex(Guid.NewGuid()) { Properties = new SortedList<string, FreeformProperty>() },
                    new FreeformVertex(Guid.NewGuid()) { Properties = new SortedList<string, FreeformProperty>() },
                    new FreeformVertex(Guid.NewGuid()) { Properties = new SortedList<string, FreeformProperty>() },
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
                // Setup the graph information.
                FreeformGraph graph = new UndirectedGraph<FreeformVertex, FreeformEdge>();
                FreeformVertex[] vertices = new FreeformVertex[]
                {
                    new FreeformVertex(Guid.NewGuid())
                    {
                        Properties = new SortedList<string, FreeformProperty>()
                        {
                            ["myNum"] = new FreeformProperty { Type = typeof(int), Value = 2},
                            ["myFruit"] = new FreeformProperty { Type = typeof(string), Value = "orange"}
                        }
                    },
                    new FreeformVertex(Guid.NewGuid()),
                    new FreeformVertex(Guid.NewGuid())
                    {
                        Properties = new SortedList<string, FreeformProperty>()
                        {
                            ["myShoeSize"] = new FreeformProperty { Type = typeof(double), Value = 11.50},
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