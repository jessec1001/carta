using System.Threading.Tasks;
using CartaCore.Graphs;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    // TODO: Implement support for pipelining this graph.
    /// <summary>
    /// The input for the <see cref="CompleteGraphOperation" /> operation.
    /// </summary>
    public struct CompleteGraphOperationIn
    {
        /// <summary>
        /// The number of vertices to use in generating the complete graph.
        /// </summary>
        [FieldRange(Minimum = 1)]
        [FieldName("Vertex Count")]
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
        [FieldName("Graph")]
        public MemoryGraph Graph { get; set; }
    }

    /// <summary>
    /// Generates an undirected complete graph with the specified number of vertices. The complete graph contains all
    /// possible edges between each pair of vertices.
    /// </summary>
    [OperationName(Display = "Generate Complete Graph", Type = "generateCompleteGraph")]
    [OperationTag(OperationTags.Graph)]
    [OperationTag(OperationTags.Synthetic)]
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
            MemoryGraph graph = new($"K_{input.VertexCount}");

            // Generate the vertices of the graph.
            for (int m = 0; m < input.VertexCount; m++)
                graph.AddVertex(new(m.ToString()));

            // Generate the edges of the graph.
            for (int m = 0; m < input.VertexCount; m++)
            {
                for (int n = 0; n < input.VertexCount; n++)
                {
                    if (m < n)
                        graph.AddEdge(new(m.ToString(), n.ToString()) { Directed = false });
                }
            }

            return Task.FromResult(new CompleteGraphOperationOut { Graph = graph });
        }
    }
}