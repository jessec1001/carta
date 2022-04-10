using System.Threading.Tasks;
using CartaCore.Graphs;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations.Graphs
{
    /// <summary>
    /// The input for the <see cref="CyclicGraphOperation" /> operation.
    /// </summary>
    public struct CyclicGraphOperationIn
    {
        /// <summary>
        /// The number of vertices to use in generating the cyclic graph.
        /// </summary>
        [FieldRange(Minimum = 1)]
        [FieldName("Vertex Count")]
        public int VertexCount { get; set; }
        /// <summary>
        /// Whether the resulting graph should be directed or undirected.
        /// </summary>
        [FieldName("Directed")]
        public bool Directed { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="CyclicGraphOperation" /> operation.
    /// </summary>
    public struct CyclicGraphOperationOut
    {
        /// <summary>
        /// The generated cyclic graph.
        /// </summary>
        [FieldName("Graph")]
        public MemoryGraph Graph { get; set; }
    }

    /// <summary>
    /// Generates a directed or undirected cyclic graph with the specified number of vertices. The cyclic graph
    /// contains edges between vertices completing a circuit that touches each vertex once.
    /// </summary>
    [OperationName(Display = "Generate Cyclic Graph", Type = "generateCyclicGraph")]
    [OperationTag(OperationTags.Graph)]
    [OperationTag(OperationTags.Synthetic)]
    [OperationHidden]
    public class CyclicGraphOperation : TypedOperation
    <
        CyclicGraphOperationIn,
        CyclicGraphOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<CyclicGraphOperationOut> Perform(CyclicGraphOperationIn input)
        {
            // Create the graph structure.
            MemoryGraph graph = new($"C_{input.VertexCount}");

            // Generate the vertices of the graph.
            for (int k = 0; k < input.VertexCount; k++)
            {
                // Create edges connected before and after the current vertex.
                int indexBefore = (k - 1 + input.VertexCount) % input.VertexCount;
                int indexAfter = (k + 1 + input.VertexCount) % input.VertexCount;

                Edge edgeBefore = new(indexBefore.ToString(), k.ToString()) { Directed = input.Directed };
                Edge edgeAfter = new(k.ToString(), indexAfter.ToString()) { Directed = input.Directed };
                Vertex vertex = new(k.ToString(), new Edge[] { edgeBefore, edgeAfter });

                // Add the vertex to the graph.
                graph.AddVertex(vertex);
            }

            return Task.FromResult(new CyclicGraphOperationOut { Graph = graph });
        }
    }
}