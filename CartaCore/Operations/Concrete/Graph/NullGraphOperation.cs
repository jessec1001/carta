using System.Threading.Tasks;
using CartaCore.Data;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="NullGraphOperation" /> operation.
    /// </summary>
    public struct NullGraphOperationIn
    {
        /// <summary>
        /// The number of vertices to use in generating the null graph.
        /// </summary>
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
        public FiniteGraph Graph { get; set; }
    }

    /// <summary>
    /// Generates an undirected null graph of the specified number of vertices. The null graph contains no edges between
    /// any of the vertices.
    /// </summary>
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
            FiniteGraph graph = new(Identity.Create($"{nameof(NullGraphOperation)}:{input.VertexCount}"), false);

            // Generate the vertices of the graph.
            for (int k = 0; k < input.VertexCount; k++)
            {
                // Add the vertex to the graph.
                Vertex vertex = new(Identity.Create(k));
                graph.AddVertex(vertex);
            }

            return Task.FromResult(new NullGraphOperationOut { Graph = graph });
        }
    }
}