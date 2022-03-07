using System.Threading.Tasks;
using CartaCore.Integration.Synthetic;
using CartaCore.Operations.Attributes;

// TODO: Use the parameters for the graph generation as the input fields.
// TODO: Improve support for these synthetic graph generators.
namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="InfiniteDirectedGraphOperation" /> operation.
    /// </summary>
    public struct InfiniteDirectedGraphOperationIn { }
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
            return Task.FromResult(new InfiniteDirectedGraphOperationOut() { Graph = new InfiniteDirectedGraph() });
        }
    }
}