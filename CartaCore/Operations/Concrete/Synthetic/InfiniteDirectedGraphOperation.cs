using System.Threading.Tasks;
using CartaCore.Data;
using CartaCore.Integration.Synthetic;

namespace CartaCore.Operations
{
    public struct InfiniteDirectedGraphOperationIn { }
    public struct InfiniteDirectedGraphOperationOut
    {
        public Graph Graph { get; set; }
    }

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