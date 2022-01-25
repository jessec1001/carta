using System.Threading.Tasks;
using CartaCore.Data;
using CartaCore.Integration.Synthetic;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    public struct InfiniteDirectedGraphOperationIn { }
    public struct InfiniteDirectedGraphOperationOut
    {
        public Graph Graph { get; set; }
    }

    /// <summary>
    /// Creates a synthetic infinite directed graph.
    /// </summary>
    [OperationName(Display = "Infinite Directed Graph", Type = "syntheticIdg")]
    public class InfiniteDirectedGraphOperation : TypedOperation
    <
        InfiniteDirectedGraphOperationIn,
        InfiniteDirectedGraphOperationOut
    >
    {
        public override Task<InfiniteDirectedGraphOperationOut> Perform(InfiniteDirectedGraphOperationIn input)
        {
            return Task.FromResult(new InfiniteDirectedGraphOperationOut() { Graph = new InfiniteDirectedGraph() });
        }
    }
}