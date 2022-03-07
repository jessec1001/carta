using System.Threading.Tasks;
using CartaCore.Graphs;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations.Graphs
{
    /// <summary>
    /// The input for the <see cref="NullGraphOperation" /> operation.
    /// </summary>
    public struct NullGraphOperationIn
    {
        /// <summary>
        /// The number of vertices to use in generating the null graph.
        /// </summary>
        [FieldRange(Minimum = 1)]
        [FieldName("Vertex Count")]
        public int VertexCount { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="NullGraphOperation" /> operation.
    /// </summary>
    public struct NullGraphOperationOut
    {
        /// <summary>
        /// The generated null graph. 
        /// </summary>
        [FieldName("Graph")]
        public MemoryGraph Graph { get; set; }
    }

    /// <summary>
    /// Generates an undirected null graph of the specified number of vertices. The null graph contains no edges between
    /// any of the vertices.
    /// </summary>
    [OperationName(Display = "Generate Null Graph", Type = "generateNullGraph")]
    [OperationTag(OperationTags.Graph)]
    [OperationTag(OperationTags.Synthetic)]
    public class NullGraphOperation : TypedOperation
    <
        NullGraphOperationIn,
        NullGraphOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<NullGraphOperationOut> Perform(NullGraphOperationIn input)
        {
            // Generate the graph structure.
            MemoryGraph graph = new($"N_{input.VertexCount}");

            // Generate the vertices of the graph.
            for (int k = 0; k < input.VertexCount; k++)
            {
                // Add the vertex to the graph.
                Vertex vertex = new(k.ToString());
                graph.AddVertex(vertex);
            }

            return Task.FromResult(new NullGraphOperationOut { Graph = graph });
        }
    }
}