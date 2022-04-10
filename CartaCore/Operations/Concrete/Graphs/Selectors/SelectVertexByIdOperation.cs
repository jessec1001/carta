using System.Linq;
using System.Threading.Tasks;
using CartaCore.Graphs;
using CartaCore.Graphs.Components;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations.Graphs
{
    /// <summary>
    /// The input for the <see cref="SelectVertexByIdOperation"/> operation.
    /// </summary>
    public struct SelectVertexByIdOperationIn
    {
        /// <summary>
        /// The graph to perform the exclusion on.
        /// </summary>
        [FieldName("Graph")]
        public Graph Graph { get; set; }
        /// <summary>
        /// The identifiers of the vertices to exclude.
        /// </summary>
        [FieldRequired]
        [FieldName("Vertex IDs")]
        public string[] Ids { get; set; }
        /// <summary>
        /// Whether vertices shouold be excluded or included based on the specified identifiers. 
        /// </summary>
        [FieldDefault(false)]
        [FieldName("Exclusion Mode")]
        public bool Exclude { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="SelectVertexByIdOperation"/> operation.
    /// </summary>
    public struct SelectVertexByIdOperationOut
    {
        /// <summary>
        /// The resulting graph containing the non-excluded vertices.
        /// </summary>
        [FieldName("Graph")]
        public Graph Graph { get; set; }
    }

    /// <summary>
    /// Filters the vertices of a graph by matching their identifier.
    /// </summary>
    [OperationName(Display = "Filter Vertices by ID", Type = "filterVertexById")]
    [OperationTag(OperationTags.Graph)]
    [OperationSelector("id")]
    public class SelectVertexByIdOperation : TypedOperation
    <
        SelectVertexByIdOperationIn,
        SelectVertexByIdOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<SelectVertexByIdOperationOut> Perform(SelectVertexByIdOperationIn input)
        {
            // Check if the identifiers list is null or empty.
            if (input.Ids is null || input.Ids.Length == 0)
                return Task.FromResult(new SelectVertexByIdOperationOut() { Graph = input.Graph });

            // Attach the filter component to the graph.
            Graph graph = input.Graph;
            FilterVertexComponent component = new(
                input.Exclude
                    ? (vertex) => Task.FromResult(!input.Ids.Any((id) => vertex.Id == id))
                    : (vertex) => Task.FromResult(input.Ids.Any((id) => vertex.Id == id))
            );
            component.Attach(graph);

            return Task.FromResult(new SelectVertexByIdOperationOut() { Graph = graph });
        }
    }
}