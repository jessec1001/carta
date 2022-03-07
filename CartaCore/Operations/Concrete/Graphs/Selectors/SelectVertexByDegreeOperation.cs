using System.Linq;
using System.Threading.Tasks;
using CartaCore.Graphs;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations.Graphs
{
    /// <summary>
    /// The input for the <see cref="SelectVertexByDegreeOperation"/> operation.
    /// </summary>
    public struct SelectVertexByDegreeOperationIn
    {
        /// <summary>
        /// The graph containing vertices to filter.
        /// </summary>
        [FieldName("Graph")]
        public Graph Graph { get; set; }
        /// <summary>
        /// The [indegree](https://en.wikipedia.org/wiki/Directed_graph#Indegree_and_outdegree) of vertices to filter.
        /// - If set to zero, only the vertices with no directed in-edges will be selected.
        /// - If left unset, vertices with any number of in-edges will be selected.
        /// </summary>
        [FieldRange(Minimum = 0)]
        [FieldName("In-Degree")]
        public int? InDegree { get; set; }
        /// <summary>
        /// The [outdegree](https://en.wikipedia.org/wiki/Directed_graph#Indegree_and_outdegree) of vertices to select.
        /// - If set to zero, only the vertices with no directed out-edges will be selected.
        /// - If left unset, vertices with any number of out-edges will be selected.
        /// </summary>
        [FieldRange(Minimum = 0)]
        [FieldName("Out-Degree")]
        public int? OutDegree { get; set; }
        /// <summary>
        /// Whether vertices shouold be excluded or included based on the specified identifiers. 
        /// </summary>
        [FieldDefault(false)]
        [FieldName("Exclusion Mode")]
        public bool Exclude { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="SelectVertexByDegreeOperation"/> operation.
    /// </summary>
    public struct SelectVertexByDegreeOperationOut
    {
        /// <summary>
        /// The resulting graph containing only the selected vertices.
        /// </summary>
        [FieldName("Graph")]
        public Graph Graph { get; set; }
    }

    /// <summary>
    /// Filters the vertices of a graph by the number of edges into and out of them.
    /// </summary>
    [OperationName(Display = "Filter Vertices by Degree", Type = "filterVertexByDegree")]
    [OperationTag(OperationTags.Graph)]
    [OperationSelector("degree")]
    public class SelectVertexByDegreeOperation : TypedOperation
    <
        SelectVertexByDegreeOperationIn,
        SelectVertexByDegreeOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<SelectVertexByDegreeOperationOut> Perform(SelectVertexByDegreeOperationIn input)
        {
            // TODO: Reimplement the rooted graph optimization.

            // Attach the filter component to the graph.
            Graph graph = input.Graph;
            FilterVertexComponent component = new((vertex) =>
            {
                if (input.InDegree is not null && vertex.InEdges.Count() != input.InDegree)
                    return Task.FromResult(false);
                if (input.OutDegree is not null && vertex.OutEdges.Count() != input.OutDegree)
                    return Task.FromResult(false);
                return Task.FromResult(true);
            });
            component.Attach(graph);

            return Task.FromResult(new SelectVertexByDegreeOperationOut() { Graph = graph });
        }
    }
}