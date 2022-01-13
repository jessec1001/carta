using System.Linq;
using System.Threading.Tasks;
using CartaCore.Data;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="IncludeVertexByIdOperation"/> operation.
    /// </summary>
    public struct IncludeVertexByIdOperationIn
    {
        /// <summary>
        /// The graph to perform the inclusion on.
        /// </summary>
        [OperationSelectorGraph]
        public Graph Graph { get; set; }
        /// <summary>
        /// The identifiers of the vertices to include.
        /// </summary>
        public string[] Ids { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="IncludeVertexByIdOperation"/> operation.
    /// </summary>
    public struct IncludeVertexByIdOperationOut
    {
        /// <summary>
        /// The resulting graph containing the included vertices.
        /// </summary>
        [OperationSelectorGraph]
        public Graph Graph { get; set; }
    }

    /// <summary>
    /// Includes the vertices of a graph by matching their identifier.
    /// </summary>
    [OperationName(Display = "Include Vertices by ID", Type = "includeVertexById")]
    [OperationTag(OperationTags.Graph)]
    [OperationSelector("include")]
    public class IncludeVertexByIdOperation : TypedOperation
    <
        IncludeVertexByIdOperationIn,
        IncludeVertexByIdOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<IncludeVertexByIdOperationOut> Perform(IncludeVertexByIdOperationIn input)
        {
            // Check if the identifiers list is null or empty.
            if (input.Ids is null || input.Ids.Length == 0)
                return Task.FromResult(new IncludeVertexByIdOperationOut() { Graph = input.Graph });

            // Create a filter graph and use it to filter the vertices.
            return Task.FromResult(
                new IncludeVertexByIdOperationOut()
                {
                    Graph = new FilterGraph
                    (
                        input.Graph,
                        (vertex) => Task.FromResult(input.Ids.Any((id) => vertex.Identifier.Equals(id)))
                    )
                });
        }
    }
}