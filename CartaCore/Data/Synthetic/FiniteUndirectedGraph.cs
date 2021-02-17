using System;
using System.Collections.Generic;
using System.Linq;

using QuikGraph;

using CartaCore.Data.Freeform;
using CartaCore.Utility;

namespace CartaCore.Data.Synthetic
{
    /// <summary>
    /// Represents graph data of a random, finite, undirected graph. Both the vertices and edges are randomly generated
    /// and connected.
    /// </summary>
    public class FiniteUndirectedGraph : FreeformGraph, IParameterizedGraph<FiniteUndirectedGraphParameters>
    {
        /// <inheritdoc />
        public FiniteUndirectedGraphParameters Parameters { get; set; }

        /// <summary>
        /// The entirety of the graph generated.
        /// </summary>
        /// <value>The randomly generated graph.</value>
        private IMutableVertexAndEdgeSet<FreeformVertex, FreeformEdge> Graph { get; set; }

        /// <summary>
        /// Creates a new random sampled, finite, undirected graph with the specified parameters.
        /// </summary>
        /// <param name="parameters">The parameters to generate the graph with.</param>
        public FiniteUndirectedGraph(FiniteUndirectedGraphParameters parameters = default(FiniteUndirectedGraphParameters))
        {
            // We generate all the graph data after setting the parameters.
            Parameters = parameters;
            Graph = GenerateGraph();
        }

        /// <summary>
        /// Generates the random graph data.
        /// </summary>
        /// <returns>The random graph data.</returns>
        protected UndirectedGraph<FreeformVertex, FreeformEdge> GenerateGraph()
        {
            // Random number generator that is seeded with whatever seed was specified.
            CompoundRandom random = new CompoundRandom(Parameters.Seed);

            // Generate the random number of vertices and edges.
            int numVertices = random.NextInt(Parameters.MinVertices, Parameters.MaxVertices + 1);
            int possibleEdges = numVertices * (numVertices - 1) / 2;
            int numEdges = random.NextInt(
                Math.Min(possibleEdges, Parameters.MinEdges),
                Math.Min(possibleEdges, Parameters.MaxEdges)
            );

            // Generate the vertices.
            List<FreeformVertex> vertices = new List<FreeformVertex>(
                Enumerable
                .Range(0, numVertices)
                .Select(_ => new FreeformVertex(FreeformIdentity.Create(random.NextGuid())))
            );

            // Generate the edges to randomly select from.
            // This uses a cartesian product without doubles or reversals.
            int edgeCount = 0;
            LinkedList<FreeformEdge> edges = new LinkedList<FreeformEdge>(
                vertices
                .SelectMany(
                    (vertexA, indexA) => vertices
                        .Where((vertexB, indexB) => indexA < indexB),
                    (a, b) => new FreeformEdge(a.Identifier, b.Identifier, edgeCount++)
                )
            );

            // Randomly select the edges from our list of plausible edges.
            LinkedList<FreeformEdge> edgesSelected = new LinkedList<FreeformEdge>();
            for (int e = 0; e < numEdges; e++)
            {
                // This here is probably a bit inefficient and sloppy but it performs better than reallocating
                // arrays and this is the best we can accomplish with a linked list.
                int offset = random.NextInt(edges.Count);
                LinkedListNode<FreeformEdge> edgesNode = edges.First;
                for (int j = 0; j < offset; j++)
                    edgesNode = edgesNode.Next;

                // Add selected node to selected edges and remove from possible edges.
                edgesSelected.AddLast(edgesNode.Value);
                edges.Remove(edgesNode);
            }

            // We convert the edge list to a undirected graph and return.
            UndirectedGraph<FreeformVertex, FreeformEdge> graph = new UndirectedGraph<FreeformVertex, FreeformEdge>();
            graph.AddVertexRange(vertices);
            graph.AddEdgeRange(edgesSelected);
            return graph;
        }

        #region FreeformGraph
        /// <inheritdoc />
        public override bool IsDirected => Graph.IsDirected;
        /// <inheritdoc />
        public override bool AllowParallelEdges => Graph.AllowParallelEdges;

        /// <inheritdoc />
        public override bool IsVerticesEmpty => Graph.IsVerticesEmpty;
        /// <inheritdoc />
        public override bool IsEdgesEmpty => Graph.IsEdgesEmpty;

        /// <inheritdoc />
        public override int VertexCount => Graph.VertexCount;
        /// <inheritdoc />
        public override int EdgeCount => Graph.EdgeCount;

        /// <inheritdoc />
        public override IEnumerable<FreeformVertex> Vertices => Graph.Vertices;
        /// <inheritdoc />
        public override IEnumerable<FreeformEdge> Edges => Graph.Edges;

        /// <inheritdoc />
        public override bool ContainsEdge(FreeformEdge edge)
        {
            return Graph.ContainsEdge(edge);
        }
        /// <inheritdoc />
        public override bool ContainsVertex(FreeformVertex vertex)
        {
            return Graph.ContainsVertex(vertex);
        }
        #endregion
    }
}