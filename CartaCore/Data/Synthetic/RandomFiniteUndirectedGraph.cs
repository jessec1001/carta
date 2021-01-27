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
    public class RandomFiniteUndirectedGraph : ISampledGraph, IOptionsGraph<RandomFiniteUndirectedGraphOptions>
    {
        /// <inheritdoc />
        public RandomFiniteUndirectedGraphOptions Options { get; set; }

        /// <summary>
        /// The entirety of the graph generated.
        /// </summary>
        /// <value>The randomly generated graph.</value>
        private FreeformGraph Graph { get; set; }

        /// <summary>
        /// Creates a new random sampled, finite, undirected graph with the specified parameters.
        /// </summary>
        /// <param name="options">The options to generate the graph with.</param>
        public RandomFiniteUndirectedGraph(RandomFiniteUndirectedGraphOptions options = default(RandomFiniteUndirectedGraphOptions))
        {
            // We generate all the graph data after setting the parameters.
            Options = options;
            Graph = GenerateGraph();
        }

        /// <summary>
        /// Generates the random graph data.
        /// </summary>
        /// <returns>The random graph data.</returns>
        protected FreeformGraph GenerateGraph()
        {
            // Random number generator that is seeded with whatever seed was specified.
            CompoundRandom random = new CompoundRandom(Options.Seed);

            // Generate the random number of vertices and edges.
            int numVertices = random.NextInt(Options.MinVertices, Options.MaxVertices + 1);
            int possibleEdges = numVertices * (numVertices - 1) / 2;
            int numEdges = random.NextInt(
                Math.Min(possibleEdges, Options.MinEdges),
                Math.Min(possibleEdges, Options.MaxEdges)
            );

            // Generate the vertices.
            List<FreeformVertex> vertices = new List<FreeformVertex>(
                Enumerable
                .Range(0, numVertices)
                .Select(_ => new FreeformVertex
                {
                    Id = random.NextGuid(),
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