using System;
using System.Threading.Tasks;
using CartaCore.Integration.Synthetic;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="FiniteUndirectedGraphOperation" /> operation.
    /// </summary>
    public struct FiniteUndirectedGraphOperationIn
    {
        /// <summary>
        /// A seed used for the random generation of the graph.
        /// If no seed is specified, a seed will be generated.
        /// </summary>
        [FieldName("Seed")]
        public ulong? Seed { get; set; }
        /// <summary>
        /// Whether the vertices in the graph should be labeled.
        /// </summary>
        [FieldDefault(true)]
        [FieldName("Labeled")]
        public bool Labeled { get; set; }

        /// <summary>
        /// The number of vertices that should be generated.
        /// </summary>
        [FieldDefault(10)]
        [FieldRange(Minimum = 0)]
        [FieldName("Vertex Count")]
        public int VertexCount { get; set; }
        /// <summary>
        /// The number of edges that should be generated.
        /// </summary>
        [FieldDefault(30)]
        [FieldRange(Minimum = 0)]
        [FieldName("Edge Count")]
        public int EdgeCount { get; set; }
    }
    /// <summary>
    /// The output of the <see cref="FiniteUndirectedGraphOperation" /> operation.
    /// </summary>
    public struct FiniteUndirectedGraphOperationOut
    {
        /// <summary>
        /// The generated synthetic graph.
        /// </summary>
        [FieldName("Graph")]
        public FiniteUndirectedGraph Graph { get; set; }
    }

    /// <summary>
    /// Creates a synthetic finite undirected graph.
    /// </summary>
    [OperationName(Display = "Finite Undirected Graph", Type = "syntheticFug")]
    [OperationTag(OperationTags.Graph)]
    [OperationTag(OperationTags.Synthetic)]
    public class FiniteUndirectedGraphOperation : TypedOperation
    <
        FiniteUndirectedGraphOperationIn,
        FiniteUndirectedGraphOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<FiniteUndirectedGraphOperationOut> Perform(FiniteUndirectedGraphOperationIn input)
        {
            FiniteUndirectedGraph graph = new(new FiniteUndirectedGraphParameters()
            {
                Seed = input.Seed ?? (ulong)(new Random().Next()),
                Labeled = input.Labeled,
                VertexCount = input.VertexCount,
                EdgeCount = input.EdgeCount
            });
            return Task.FromResult(new FiniteUndirectedGraphOperationOut() { Graph = graph });
        }
    }
}