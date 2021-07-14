using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using NJsonSchema.Annotations;

using CartaCore.Data;
using CartaCore.Serialization;

namespace CartaCore.Workflow.Selection
{
    /// <summary>
    /// Selects vertices based on a regular expression match of their labels.
    /// </summary>
    [JsonSchemaFlatten]
    [DataContract]
    [DiscriminantDerived("vertexName")]
    [DiscriminantSemantics(Name = "Select Vertex Name", Group = "Vertex")]
    public class SelectorVertexName : SelectorRegexBase
    {
        /// <summary>
        /// The pattern used to match vertex labels to select them.
        /// - If selecting by inclusion of text such as matching "Part" in labels of the form "01_Part_5692", just enter "Part".
        /// - If selecting by [regular expression](https://regexr.com/) such as matching hexadecimal labels, enter your regular expression surrounded by forward slashes like "/[0-9a-f]+/".
        /// </summary>
        [JsonSchemaExtensionData("format", "regex")]
        [DataMember(Name = "regexPattern")]
        [Display(Name = "Pattern")]
        [Required]
        public string RegexPattern
        {
            get => Pattern;
            set => Pattern = value;
        }

        /// <inheritdoc />
        public override Task<bool> ContainsVertex(IVertex vertex)
        {
            if (Regex is null) return Task.FromResult(true);
            return Task.FromResult(!(vertex.Label is null) && Regex.IsMatch(vertex.Label));
        }
    }
}