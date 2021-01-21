using System;
using System.Collections.Generic;
using System.Linq;

using QuikGraph;

using CartaCore.Data;
using CartaCore.Utility;

namespace CartaCore.Data.Synthetic
{
    using FreeformGraph = IEdgeListAndIncidenceGraph<FreeformVertex, Edge<FreeformVertex>>;

    public class RandomFiniteUndirectedGraph
    {
        private ulong Seed;
        /// <summary>
        /// The minimum number (inclusive) of vertices a generated graph can have. 
        /// </summary>
        private int MinVertices { get; set; }
        /// <summary>
        /// The maximum number (inclusive) of vertices a generated graph can have.
        /// </summary>
        private int MaxVertices { get; set; }
        /// <summary>
        /// The minimum number (inclusive) of edges a generated graph can have. 
        /// </summary>
        private int MinEdges { get; set; }
        /// <summary>
        /// The maximum number (inclusive) of edges a generated graph can have.
        /// </summary>
        private int MaxEdges { get; set; }

        private FreeformGraph Graph { get; set; }

        public RandomFiniteUndirectedGraph(
            ulong seed = 0,
            int minVertices = 1, int maxVertices = 10,
            int minEdges = 0, int maxEdges = 50
        )
        {
            Seed = seed;
            MinVertices = minVertices;
            MaxVertices = maxVertices;
            MinEdges = minEdges;
            MaxEdges = maxEdges;

            Graph = GenerateGraph();
        }

        protected FreeformGraph GenerateGraph()
        {
            // Random number generator. Seeded if a seed was specified.
            CompoundRandom random = new CompoundRandom(Seed);

            // Generate the random number of vertices and edges.
            int numVertices = random.NextInt(MinVertices, MaxVertices + 1);
            int possibleEdges = numVertices * (numVertices - 1) / 2;
            int numEdges = random.NextInt(
                Math.Min(possibleEdges, MinEdges),
                Math.Min(possibleEdges, MaxEdges)
            );

            // Generate the vertices.
            List<FreeformVertex> vertices = new List<FreeformVertex>(
                Enumerable
                .Range(0, numVertices)
                .Select(_ => new FreeformVertex
                {
                    Id = Guid.NewGuid(),
                    Properties = new SortedList<string, FreeformVertexProperty>()
                })
            );

            // Generate the edges to randomly select from.
            // This uses a cartesian product without doubles or reversals.
            LinkedList<Edge<FreeformVertex>> edges = new LinkedList<Edge<FreeformVertex>>(
                vertices
                .SelectMany(
                    (vertexA, indexA) => vertices
                        .Where((vertexB, indexB) => indexA < indexB),
                    (a, b) => new Edge<FreeformVertex>(a, b)
                )
            );

            // Randomly select the edges from our list of plausible edges.
            LinkedList<Edge<FreeformVertex>> edgesSelected = new LinkedList<Edge<FreeformVertex>>();
            for (int e = 0; e < numEdges; e++)
            {
                // This here is probably a bit inefficient and sloppy but it performs better than reallocating
                // arrays and this is the best we can accomplish with a linked list.
                int offset = random.NextInt(edges.Count);
                LinkedListNode<Edge<FreeformVertex>> edgesNode = edges.First;
                for (int j = 0; j < offset; j++)
                    edgesNode = edgesNode.Next;

                // Add selected node to selected edges and remove from possible edges.
                edgesSelected.AddLast(edgesNode.Value);
                edges.Remove(edgesNode);
            }

            // We convert the edge list to a undirected graph and return.
            AdjacencyGraph<FreeformVertex, Edge<FreeformVertex>> graph = new AdjacencyGraph<FreeformVertex, Edge<FreeformVertex>>();
            graph.AddVertexRange(vertices);
            graph.AddEdgeRange(edgesSelected);
            return graph;
        }

        public bool IsFinite() => true;
        public FreeformVertex GetVertexProperties(Guid id)
        {
            return Graph.Vertices
                .Where(vertex => vertex.Id == id)
                .FirstOrDefault();
        }
        public IEnumerable<Edge<FreeformVertex>> GetVertexEdges(Guid id)
        {
            return Graph.Edges
                .Where(edge =>
                    edge.Source.Id == id ||
                    edge.Target.Id == id);
        }
        public FreeformGraph GetGraph() => Graph;
    }
}