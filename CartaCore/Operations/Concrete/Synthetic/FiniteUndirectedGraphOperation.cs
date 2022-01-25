using System.Threading.Tasks;
using CartaCore.Data;
using CartaCore.Integration.Synthetic;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    public struct FiniteUndirectedGraphOperationIn { }
    public struct FiniteUndirectedGraphOperationOut
    {
        public Graph Graph { get; set; }
    }

    /// <summary>
    /// Creates a synthetic finite undirected graph.
    /// </summary>
    [OperationName(Display = "Finite Undirected Graph", Type = "syntheticFug")]
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