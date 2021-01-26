using System;
using System.Collections.Generic;
using System.Linq;

using QuikGraph;

using CartaCore.Utility;

namespace CartaCore.Data.Synthetic
{
    using FreeformGraph = IEdgeListAndIncidenceGraph<FreeformVertex, Edge<FreeformVertex>>;

    /// <summary>
    /// Represents graph data of a random, finite, undirected graph. Both the vertices and edges are randomly generated
    /// and connected.
    /// </summary>
    public class RandomFiniteUndirectedGraph : ISampledGraph
    {
        /// <summary>
        /// The seed for random generation of the graph.
        /// </summary>
        /// <value>The random seed.</value>
        private ulong Seed;
        /// <summary>
        /// The minimum number (inclusive) of vertices a generated graph can have. 
        /// </summary>
        /// <value>The mininum number of vertices.</value>
        private int MinVertices { get; set; }
        /// <summary>
        /// The maximum number (inclusive) of vertices a generated graph can have.
        /// </summary>
        /// <value>The maximum number of vertices.</value>
        private int MaxVertices { get; set; }
        /// <summary>
        /// The minimum number (inclusive) of edges a generated graph can have. 
        /// </summary>
        /// <value>The mininum number of edges.</value>
        private int MinEdges { get; set; }
        /// <summary>
        /// The maximum number (inclusive) of edges a generated graph can have.
        /// </summary>
        /// <value>The maximum number of vertices.</value>
        private int MaxEdges { get; set; }

        /// <summary>
        /// The entirety of the graph generated.
        /// </summary>
        /// <value>The randomly generated graph.</value>
        private FreeformGraph Graph { get; set; }

        /// <summary>
        /// Creates a new random sampled, finite, undirected graph with the specified parameters.
        /// </summary>
        /// <param name="seed">The seed for random generation.</param>
        /// <param name="minVertices">The minimum (inclusive) number of vertices.</param>
        /// <param name="maxVertices">The maximum (inclusive) number of vertices.</param>
        /// <param name="minEdges">The minimum (inclusive) number of edges.</param>
        /// <param name="maxEdges">The maximum (inclusive) number of edges.</param>
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

            // We generate all the graph data after setting the parameters.
            Graph = GenerateGraph();
        }

        /// <summary>
        /// Generates the random graph data.
        /// </summary>
        /// <returns>The random graph data.</returns>
        protected FreeformGraph GenerateGraph()
        {
            // Random number generator that is seeded with whatever seed was specified.
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

        /// <summary>
        /// Whether the graph has a finite or infinite number of vertices and edges.
        /// </summary>
        /// <value>Always <c>true</c>.</value>
        public bool IsFinite => true;

        /// <inheritdoc />
        public FreeformGraph GetEntire() => Graph;

        /// <inheritdoc />
        public FreeformVertex GetProperties(Guid id)
        {
            // Return the properties of the vertex whose ID matches.
            return Graph.Vertices
                .Where(vertex => vertex.Id == id)
                .FirstOrDefault();
        }
        /// <inheritdoc />
        public IEnumerable<Edge<FreeformVertex>> GetEdges(Guid id)
        {
            // Return the out-edges and in-edges of the vertex whose ID matches.
            // It doesn't matter whether the edge is an out-edge or an in-edge because this is an undirected graph.
            return Graph.Edges
                .Where(edge =>
                    edge.Source.Id == id ||
                    edge.Target.Id == id);
        }
    }
}