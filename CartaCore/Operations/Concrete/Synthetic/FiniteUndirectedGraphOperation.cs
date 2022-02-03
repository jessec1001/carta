using System.Threading.Tasks;
using CartaCore.Data;
using CartaCore.Integration.Synthetic;
using CartaCore.Operations.Attributes;

// TODO: Improve support for these synthetic graph generators.
namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="FiniteUndirectedGraphOperation" /> operation.
    /// </summary>
    public struct FiniteUndirectedGraphOperationIn { }
    /// <summary>
    /// The output of the <see cref="FiniteUndirectedGraphOperation" /> operation.
    /// </summary>
    public struct FiniteUndirectedGraphOperationOut
    {
        /// <summary>
        /// The generated synthetic graph.
        /// </summary>
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
        /// <inheritdoc />
        public override Task<FiniteUndirectedGraphOperationOut> Perform(FiniteUndirectedGraphOperationIn input)
        {
            return Task.FromResult(
                new FiniteUndirectedGraphOperationOut()
                { Graph = new FiniteUndirectedGraph(new FiniteUndirectedGraphParameters()) }
            );
        }
    }
}