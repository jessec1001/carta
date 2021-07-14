using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using CartaCore.Data;
using CartaCore.Serialization;

using NJsonSchema.Annotations;

namespace CartaCore.Workflow.Selection
{
    /// <summary>
    /// Represents a selection of properties based on a regular expression match of property identifiers.
    /// </summary>
    [JsonSchemaFlatten]
    [DataContract]
    [DiscriminantDerived("propertyName")]
    [DiscriminantSemantics(Name = "Select Property Name", Group = "Property")]
    public class SelectorPropertyName : SelectorRegexBase
    {
        /// <summary>
        /// The pattern used to match property names to select them.
        /// - If selecting by inclusion of text such as matching "Elastic" in names of the form "Elastic Potential" or "Elasticity", just enter "Elastic".
        /// - If selecting by [regular expression](https://regexr.com/) such as matching hexadecimal names, enter your regular expression surrounded by forward slashes like "/[0-9a-f]+/".
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
        public override Task<bool> ContainsProperty(Property property)
        {
            if (Regex is null) return Task.FromResult(true);
            return Task.FromResult(Regex.IsMatch(property.Identifier.ToString()));
        }
    }
}