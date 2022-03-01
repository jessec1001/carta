using System;
using System.Collections.Generic;
using System.Linq;
using CartaCore.Graphs;
using CartaCore.Graphs.Components;
using CartaCore.Statistics;

namespace CartaCore.Integration.Synthetic
{
    /// <summary>
    /// Represents graph data of a random, finite, undirected graph. Both the vertices and edges are randomly generated
    /// and connected.
    /// </summary>
    public class FiniteUndirectedGraph :
        MemoryGraph<Vertex, Edge>,
        IParameterizedComponent<FiniteUndirectedGraphParameters>
    {
        /// <inheritdoc />
        public FiniteUndirectedGraphParameters Parameters { get; set; }

        /// <summary>
        /// Creates a new random sampled, finite, undirected graph with the specified parameters.
        /// </summary>
        /// <param name="parameters">The parameters to generate the graph with.</param>
        public FiniteUndirectedGraph(FiniteUndirectedGraphParameters parameters = default)
            : base(nameof(FiniteUndirectedGraph))
        {
            // We generate all the graph data after setting the parameters.
            Parameters = parameters;
            GenerateGraph();
        }

        /// <summary>
        /// Generates the random graph data.
        /// </summary>
        /// <returns>The random graph data.</returns>
        protected void GenerateGraph()
        {
            // Random number generator that is seeded with whatever seed was specified.
            CompoundRandom random = new(Parameters.Seed);

            // Generate the random number of vertices and edges.
            int numVertices = Math.Max(Parameters.VertexCount, 0);
            int possibleEdges = numVertices * (numVertices - 1) / 2;
            int numEdges = Math.Clamp(Parameters.EdgeCount, 0, possibleEdges);

            // Generate the vertices.
            List<Vertex> vertices = new(
                Enumerable
                .Range(0, numVertices)
                .Select
                (_ => new Vertex(random.NextGuid().ToString())
                    {
                        Label = Parameters.Labeled ? random.NextPsuedoword() : null
                    }
                )
            );

            // Generate the edges to randomly select from.
            // This uses a cartesian product without doubles or reversals.
            LinkedList<Edge> edges = new(
                vertices
                .SelectMany(
                    (vertexA, indexA) => vertices
                        .Where((vertexB, indexB) => indexA < indexB),
                    (a, b) => new Edge(a, b) { Directed = false }
                )
            );

            // Randomly select the edges from our list of plausible edges.
            LinkedList<Edge> edgesSelected = new();
            for (int e = 0; e < numEdges; e++)
            {
                // This here is probably a bit inefficient and sloppy but it performs better than reallocating
                // arrays and this is the best we can accomplish with a linked list.
                int offset = random.NextInt(edges.Count);
                LinkedListNode<Edge> edgesNode = edges.First;
                for (int j = 0; j < offset; j++)
                    edgesNode = edgesNode.Next;

                // Add selected node to selected edges and remove from possible edges.
                edgesSelected.AddLast(edgesNode.Value);
                edges.Remove(edgesNode);
            }

            // We add the vertices and edges directly to this object.
            AddVertexRange(vertices);
            AddEdgeRange(edgesSelected);
        }
    }
}