using System.Threading.Tasks;
using CartaCore.Data;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="CyclicGraphOperation" /> operation.
    /// </summary>
    public struct CyclicGraphOperationIn
    {
        /// <summary>
        /// The number of vertices to use in generating the cyclic graph.
        /// </summary>
        public int VertexCount { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="CyclicGraphOperation" /> operation.
    /// </summary>
    public struct CyclicGraphOperationOut
    {
        /// <summary>
        /// The generated cyclic graph.
        /// </summary>
        public FiniteGraph Graph { get; set; }
    }

    /// <summary>
    /// Generates an undirected cyclic graph with the specified number of vertices. The cyclic graph contains edges
    /// between vertices completing a circuit that touches each vertex once.
    /// </summary>
    [OperationName(Display = "Generate Cyclic Graph", Type = "generateCyclicGraph")]
    [OperationTag(OperationTags.Graph)]
    [OperationTag(OperationTags.Synthetic)]
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
            FiniteGraph graph = new(Identity.Create($"{nameof(CyclicGraphOperation)}:{input.VertexCount}"), false);

            // Generate the vertices of the graph.
            for (int k = 0; k < input.VertexCount; k++)
            {
                // Create edges connected before and after the current vertex.
                int indexBefore = (k - 1 + input.VertexCount) % input.VertexCount;
                int indexAfter = (k + 1 + input.VertexCount) % input.VertexCount;

                Edge edgeBefore = new(Identity.Create(indexBefore), Identity.Create(k));
                Edge edgeAfter = new(Identity.Create(k), Identity.Create(indexAfter));
                Vertex vertex = new(Identity.Create(k), new Edge[] { edgeBefore, edgeAfter });

                // Add the vertex to the graph.
                graph.AddVertex(vertex);
            }

            return Task.FromResult(new CyclicGraphOperationOut { Graph = graph });
        }
    }
}