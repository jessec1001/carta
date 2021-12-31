using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CartaCore.Data;
using CartaCore.Utilities;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="FilterVertexByNameOperation"/> operation.
    /// </summary>
    public struct FilterVertexByNameOperationIn
    {
        /// <summary>
        /// The graph to perform the filter on.
        /// </summary>
        public Graph Graph { get; set; }
        /// <summary>
        /// The pattern used to match vertices by name.
        /// - If selecting by inclusion of text such as matching "Part" in labels of the form "01_Part_5692", just enter "Part".
        /// - If selecting by [regular expression](https://regexr.com/) such as matching hexadecimal labels, enter your regular expression surrounded by forward slashes like "/[0-9a-f]+/".
        /// </summary>
        public string Pattern { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="FilterVertexByNameOperation"/> operation.
    /// </summary>
    public struct FilterVertexByNameOperationOut
    {
        /// <summary>
        /// The resulting graph containing the filtered vertices.
        /// </summary>
        public Graph Graph { get; set; }
    }

    /// <summary>
    /// Filters the vertices of a graph by their name. Can utilize a regular expression to match the name.
    /// </summary>
    public class FilterVertexByNameOperation : TypedOperation
    <
        FilterVertexByNameOperationIn,
        FilterVertexByNameOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<FilterVertexByNameOperationOut> Perform(FilterVertexByNameOperationIn input)
        {
            // An empty pattern should be treated as a match-all pattern.
            if (input.Pattern is null)
                return Task.FromResult(new FilterVertexByNameOperationOut() { Graph = input.Graph });

            // Any other pattern needs to be converted to a regular expression.
            Regex regex = input.Pattern.ToRegexPattern();

            // Pass the regular expression to a filter graph and use it to filter the vertices.
            return Task.FromResult(
                new FilterVertexByNameOperationOut()
                {
                    Graph = new FilterGraph
                    (
                        input.Graph,
                        (vertex) => Task.FromResult(regex.IsMatch(vertex.Label))
                    )
                }
            );
        }
    }
}