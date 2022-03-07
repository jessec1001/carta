using System;
using System.Threading.Tasks;
using CartaCore.Integration.Synthetic;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="InfiniteDirectedGraphOperation" /> operation.
    /// </summary>
    public struct InfiniteDirectedGraphOperationIn
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
        /// The number of properties that should generated for the entire graph.
        /// </summary>
        [FieldDefault(16)]
        [FieldRange(Minimum = 0)]
        [FieldName("Property Count")]
        public int PropertyCount { get; set; }
        /// <summary>
        /// The probability that a property is included on any particular vertex.
        /// </summary>
        [FieldDefault(0.75)]
        [FieldRange(Minimum = 0.0, Maximum = 1.0)]
        [FieldName("Property Inclusion Probability")]
        public double PropertyInclusionProbability { get; set; }

        /// <summary>
        /// The number of children each vertex should have.
        /// </summary>
        [FieldDefault(2)]
        [FieldRange(Minimum = 0)]
        [FieldName("Child Count")]
        public int ChildCount { get; set; }
    }
    /// <summary>
    /// The output of the <see cref="InfiniteDirectedGraphOperation" /> operation.
    /// </summary>
    public struct InfiniteDirectedGraphOperationOut
    {
        /// <summary>
        /// The generated synthetic graph.
        /// </summary>
        [FieldName("Graph")]
        public InfiniteDirectedGraph Graph { get; set; }
    }

    /// <summary>
    /// Creates a synthetic infinite directed graph.
    /// </summary>

    [OperationName(Display = "Infinite Directed Graph", Type = "syntheticIdg")]
    [OperationTag(OperationTags.Graph)]
    [OperationTag(OperationTags.Synthetic)]
    public class InfiniteDirectedGraphOperation : TypedOperation
    <
        InfiniteDirectedGraphOperationIn,
        InfiniteDirectedGraphOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<InfiniteDirectedGraphOperationOut> Perform(InfiniteDirectedGraphOperationIn input)
        {
            InfiniteDirectedGraph graph = new(new InfiniteDirectedGraphParameters()
            {
                Seed = input.Seed ?? (ulong)(new Random().Next()),
                Labeled = input.Labeled,
                PropertyCount = input.PropertyCount,
                PropertyInclusionProbability = input.PropertyInclusionProbability,
                ChildCount = input.ChildCount
            });
            return Task.FromResult(new InfiniteDirectedGraphOperationOut() { Graph = graph });
        }
    }
}