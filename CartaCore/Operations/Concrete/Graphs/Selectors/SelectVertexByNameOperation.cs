using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CartaCore.Graphs;
using CartaCore.Extensions.String;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations.Graphs
{
    /// <summary>
    /// The input for the <see cref="SelectVertexByNameOperation"/> operation.
    /// </summary>
    public struct SelectVertexByNameOperationIn
    {
        /// <summary>
        /// The graph to perform the filter on.
        /// </summary>
        [FieldName("Graph")]
        public Graph Graph { get; set; }
        /// <summary>
        /// The pattern used to match vertices by name.
        /// - If selecting by inclusion of text such as matching "Part" in labels of the form "01_Part_5692", just enter "Part".
        /// - If selecting by [regular expression](https://regexr.com/) such as matching hexadecimal labels, enter your regular expression surrounded by forward slashes like "/[0-9a-f]+/".
        /// </summary>
        [FieldRequired]
        [FieldName("Pattern")]
        public string Pattern { get; set; }
        /// <summary>
        /// Whether vertices shouold be excluded or included based on the specified identifiers. 
        /// </summary>
        [FieldDefault(false)]
        [FieldName("Exclusion Mode")]
        public bool Exclude { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="SelectVertexByNameOperation"/> operation.
    /// </summary>
    public struct SelectVertexByNameOperationOut
    {
        /// <summary>
        /// The resulting graph containing the filtered vertices.
        /// </summary>
        [FieldName("Graph")]
        public Graph Graph { get; set; }
    }

    /// <summary>
    /// Selects the vertices of a graph by their name. Can utilize a regular expression to match the name.
    /// </summary>
    [OperationName(Display = "Select Vertices by Name", Type = "selectVertexByName")]
    [OperationTag(OperationTags.Graph)]
    [OperationSelector("name")]
    public class SelectVertexByNameOperation : TypedOperation
    <
        SelectVertexByNameOperationIn,
        SelectVertexByNameOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<SelectVertexByNameOperationOut> Perform(SelectVertexByNameOperationIn input)
        {
            // An empty pattern should be treated as a match-all pattern.
            if (input.Pattern is null)
                return Task.FromResult(new SelectVertexByNameOperationOut() { Graph = input.Graph });

            // Any other pattern needs to be converted to a regular expression.
            Regex regex = input.Pattern.ToRegexPattern();

            // Attach the filter component to the graph.
            Graph graph = input.Graph;
            FilterVertexComponent component = new((vertex) =>
                Task.FromResult(
                    vertex is Vertex { Label: string label } &&
                    regex.IsMatch(label)
                )
            );
            component.Attach(graph);

            return Task.FromResult(new SelectVertexByNameOperationOut() { Graph = graph });
        }
    }
}