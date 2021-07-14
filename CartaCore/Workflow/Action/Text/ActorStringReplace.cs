using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using NJsonSchema.Annotations;

using CartaCore.Serialization;
using CartaCore.Workflow.Utility;

namespace CartaCore.Workflow.Action
{
    /// <summary>
    /// Replaces any instance of a pattern string with a replacement string.
    /// </summary>
    [JsonSchemaFlatten]
    [DataContract]
    [DiscriminantDerived("stringReplace")]
    [DiscriminantSemantics(Name = "String Replacement", Group = "Text")]
    public class ActorStringReplace : Actor
    {
        /// <summary>
        /// The pattern used to match strings for replacement.
        /// - If searching by inclusion of text such as matching "mph" in values of the form "100 mph" or "76 mph", just enter "mph".
        /// - If searching by [regular expression](https://regexr.com/) such as matching hexadecimal values, enter your regular expression surrounded by forward slashes like "/[0-9a-f]+/".
        /// </summary>
        [JsonSchemaExtensionData("format", "regex")]
        [DataMember(Name = "pattern")]
        [Display(Name = "Pattern")]
        [Required]
        public string Pattern { get; set; }
        /// <summary>
        /// The replacement value used when the pattern string is matched.
        /// 
        /// If a [regular expression](https://regexr.com/) was used for the pattern string, you can reference capture
        /// groups by their index by using "$1" or "$3" for the first and third capture group values respectively.
        /// </summary>
        [DataMember(Name = "replacement")]
        [Display(Name = "Replacement")]
        [Required]
        public string Replacement { get; set; }

        /// <inheritdoc />
        public override Task<object> TransformValue(object value)
        {
            if (value is string str)
            {
                // Notice that we use a timeout of 1.0 seconds for this regular expression match to avoid malicious
                // regular expression attacks from slowing down the system.
                str = Regex.Replace
                (
                    str.EscapeRegex(), Pattern, Replacement,
                    RegexOptions.None,
                    TimeSpan.FromSeconds(1.0)
                );
                return Task.FromResult<object>(str);
            }
            else return Task.FromResult<object>(value);
        }
    }
}