using System.Threading.Tasks;
using CartaCore.Data;
using CartaCore.Integration.Synthetic;

namespace CartaCore.Operations
{
    public struct FiniteUndirectedGraphOperationIn { }
    public struct FiniteUndirectedGraphOperationOut
    {
        public Graph Graph { get; set; }
    }

    public class FiniteUndirectedGraphOperation : TypedOperation
    <
        FiniteUndirectedGraphOperationIn,
        FiniteUndirectedGraphOperationOut
    >
    {
        public override Task<FiniteUndirectedGraphOperationOut> Perform(FiniteUndirectedGraphOperationIn input)
        {
            return Task.FromResult(new FiniteUndirectedGraphOperationOut() { Graph = new FiniteUndirectedGraph() });
        }
    }
}