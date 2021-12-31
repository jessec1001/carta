using System.Linq;
using System.Threading.Tasks;
using CartaCore.Data;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="ExcludeVertexByIdOperation"/> operation.
    /// </summary>
    public struct ExcludeVertexByIdOperationIn
    {
        /// <summary>
        /// The graph to perform the exclusion on.
        /// </summary>
        [OperationSelectorGraph]
        public Graph Graph { get; set; }
        /// <summary>
        /// The identifiers of the vertices to exclude.
        /// </summary>
        public string[] Ids { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="ExcludeVertexByIdOperation"/> operation.
    /// </summary>
    public struct ExcludeVertexByIdOperationOut
    {
        /// <summary>
        /// The resulting graph containing the non-excluded vertices.
        /// </summary>
        [OperationSelectorGraph]
        public Graph Graph { get; set; }
    }

    /// <summary>
    /// Excludes the vertices of a graph by matching their identifier.
    /// </summary>
    [OperationSelector("exclude")]
    public class ExcludeVertexByIdOperation : TypedOperation
    <
        ExcludeVertexByIdOperationIn,
        ExcludeVertexByIdOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<ExcludeVertexByIdOperationOut> Perform(ExcludeVertexByIdOperationIn input)
        {
            // Check if the identifiers list is null or empty.
            if (input.Ids is null || input.Ids.Length == 0)
                return Task.FromResult(new ExcludeVertexByIdOperationOut() { Graph = input.Graph });

            // Create a filter graph and use it to filter the vertices.
            return Task.FromResult(
                new ExcludeVertexByIdOperationOut()
                {
                    Graph = new FilterGraph
                    (
                        input.Graph,
                        (vertex) => Task.FromResult(!input.Ids.Any((id) => vertex.Identifier.Equals(id)))
                    )
                });
        }
    }
}