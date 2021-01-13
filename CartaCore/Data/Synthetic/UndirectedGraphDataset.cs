using System;
using System.Collections.Generic;
using System.Linq;

using QuikGraph;

namespace CartaCore.Data.Synthetic
{
    public class UndirectedGraphDataset
    {
        /// <summary>
        /// The number of samples to generate from the dataset.
        /// </summary>
        public readonly int Samples;

        /// <summary>
        /// The minimum number (inclusive) of vertices a generated graph can have. 
        /// </summary>
        public readonly int MinVertices;
        /// <summary>
        /// The maximum number (inclusive) of vertices a generated graph can have.
        /// </summary>
        public readonly int MaxVertices;
        /// <summary>
        /// The minimum number (inclusive) of edges a generated graph can have. 
        /// </summary>
        public readonly int MinEdges;
        /// <summary>
        /// The maximum number (inclusive) of edges a generated graph can have.
        /// </summary>
        public readonly int MaxEdges;

        public UndirectedGraphDataset(
            int samples,
            int minVertices = 1, int maxVertices = 10,
            int minEdges = 0, int maxEdges = 50
        )
        {
            Samples = samples;

            MinVertices = minVertices;
            MaxVertices = maxVertices;
            MinEdges = minEdges;
            MaxEdges = maxEdges;
        }

        public IEnumerable<IUndirectedGraph<int, Edge<int>>> Generate(int? seed = null)
        {
            // Random number generator. Seeded if a seed was specified.
            Random rand = seed.HasValue ? new Random(seed.Value) : new Random();

            // Yield the specified number of samples.
            for (int k = 0; k < Samples; k++)
            {
                // Generate the random number of vertices and edges.
                int numVertices = rand.Next(MinVertices, MaxVertices + 1);
                int possibleEdges = numVertices * (numVertices - 1) / 2;
                int numEdges = rand.Next(
                    Math.Min(possibleEdges, MinEdges),
                    Math.Min(possibleEdges, MaxEdges)
                );

                // Generate the edges to randomly select from.
                // This uses a cartesian product without doubles or reversals.
                LinkedList<Edge<int>> edges = new LinkedList<Edge<int>>(
                    Enumerable
                    .Range(0, numVertices)
                    .SelectMany(
                        vertexA => Enumerable
                            .Range(0, numVertices)
                            .Where(vertexB => vertexA < vertexB),
                        (a, b) => new Edge<int>(a, b)
                    )
                );
                LinkedList<Edge<int>> edgesSelected = new LinkedList<Edge<int>>();
                for (int e = 0; e < numEdges; e++)
                {
                    // This here is probably a bit inefficient and sloppy but it performs better than reallocating
                    // arrays and this is the best we can accomplish with a linked list.
                    int offset = rand.Next(0, edges.Count);
                    LinkedListNode<Edge<int>> edgesNode = edges.First;
                    for (int j = 0; j < offset; j++)
                        edgesNode = edgesNode.Next;

                    // Add selected node to selected edges and remove from possible edges.
                    edgesSelected.AddLast(edgesNode.Value);
                    edges.Remove(edgesNode);
                }

                // We convert the edge list to a bidirectional graph and yield.
                yield return edgesSelected.ToUndirectedGraph<int, Edge<int>>();
            }
        }
    }
}