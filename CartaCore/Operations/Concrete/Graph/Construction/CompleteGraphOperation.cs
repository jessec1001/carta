using System.Threading.Tasks;
using CartaCore.Data;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="CompleteGraphOperation" /> operation.
    /// </summary>
    public struct CompleteGraphOperationIn
    {
        /// <summary>
        /// The number of vertices to use in generating the complete graph.
        /// </summary>
        public int VertexCount { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="CompleteGraphOperation" /> operation.
    /// </summary>
    public struct CompleteGraphOperationOut
    {
        /// <summary>
        /// The generated complete graph.
        /// </summary>
        public FiniteGraph Graph { get; set; }
    }

    /// <summary>
    /// Generates an undirected complete graph with the specified number of vertices. The complete graph contains all
    /// possible edges between each pair of vertices.
    /// </summary>
    public class CompleteGraphOperation : TypedOperation
    <
        CompleteGraphOperationIn,
        CompleteGraphOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<CompleteGraphOperationOut> Perform(CompleteGraphOperationIn input)
        {
            // Create the graph structure.
            FiniteGraph graph = new(Identity.Create($"{nameof(CompleteGraphOperation)}:{input.VertexCount}"), false);

            // Generate the vertices of the graph.
            for (int m = 0; m < input.VertexCount; m++)
            {
                // Create edges to all other vertices.
                Edge[] edges = new Edge[input.VertexCount - 1];
                for (int n = 0; n < input.VertexCount; n++)
                {
                    int edgeIndex = n < m ? n : n - 1;
                    if (m != n)
                        edges[edgeIndex] = new(Identity.Create(m), Identity.Create(n));
                }
                Vertex vertex = new(Identity.Create(m), edges);

                // Add the vertex to the graph.
                graph.AddVertex(vertex);
            }

            return Task.FromResult(new CompleteGraphOperationOut { Graph = graph });
        }
    }
}