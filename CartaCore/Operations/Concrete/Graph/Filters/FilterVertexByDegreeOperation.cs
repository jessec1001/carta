using System.Linq;
using System.Threading.Tasks;
using CartaCore.Data;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="FilterVertexByDegreeOperation"/> operation.
    /// </summary>
    public struct FilterVertexByDegreeOperationIn
    {
        /// <summary>
        /// The graph containing vertices to filter.
        /// </summary>
        public Graph Graph { get; set; }
        /// <summary>
        /// The [indegree](https://en.wikipedia.org/wiki/Directed_graph#Indegree_and_outdegree) of vertices to filter.
        /// - If set to zero, only the vertices with no directed in-edges will be selected.
        /// - If left unset, vertices with any number of in-edges will be selected.
        /// </summary>
        public int? InDegree { get; set; }
        /// <summary>
        /// The [outdegree](https://en.wikipedia.org/wiki/Directed_graph#Indegree_and_outdegree) of vertices to select.
        /// - If set to zero, only the vertices with no directed out-edges will be selected.
        /// - If left unset, vertices with any number of out-edges will be selected.
        /// </summary>
        public int? OutDegree { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="FilterVertexByDegreeOperation"/> operation.
    /// </summary>
    public struct FilterVertexByDegreeOperationOut
    {
        /// <summary>
        /// The resulting graph containing only the selected vertices.
        /// </summary>
        public Graph Graph { get; set; }
    }

    /// <summary>
    /// Filters the vertices of a graph by the number of edges into and out of them.
    /// </summary>
    public class FilterVertexByDegreeOperation : TypedOperation
    <
        FilterVertexByDegreeOperationIn,
        FilterVertexByDegreeOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<FilterVertexByDegreeOperationOut> Perform(FilterVertexByDegreeOperationIn input)
        {
            return Task.FromResult(
                new FilterVertexByDegreeOperationOut()
                {
                    Graph = new FilterGraph
                    (
                        input.Graph,
                        (vertex) => {
                            if (vertex is not Vertex vertexInstance) return Task.FromResult(false);
                            if (input.InDegree is not null && vertexInstance.InEdges.Count() != input.InDegree)
                                return Task.FromResult(false);
                            if (input.OutDegree is not null && vertexInstance.OutEdges.Count() != input.OutDegree)
                                return Task.FromResult(false);
                            return Task.FromResult(true);
                        }
                    )
                }
            );
        }
    }
}