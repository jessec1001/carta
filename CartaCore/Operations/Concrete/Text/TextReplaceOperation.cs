using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CartaCore.Extensions.String;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations.Text
{
    /// <summary>
    /// The input for the <see cref="TextReplaceOperation"/> operation.
    /// </summary>
    public struct TextReplaceOperationIn
    {
        /// <summary>
        /// The pattern to match where the text should be replaced.
        /// - If matching by plain text, just enter the text you want to match.
        /// - If matching by [regular expression](https://regexr.com/), enter your regular expression surrounded by forward slashes like "/[0-9a-f]+/".
        /// </summary>
        [FieldRequired]
        [FieldName("Pattern")]
        public string Pattern { get; set; }
        /// <summary>
        /// The replacement text.
        /// If a [regular expression](https://regexr.com/) was used for the pattern string, you can reference capture
        /// groups by their index by using "$1" or "$3" for the first and third capture group values respectively.
        /// </summary>
        [FieldRequired]
        [FieldName("Replacement")]
        public string Replacement { get; set; }
        /// <summary>
        /// The text to perform replacements on.
        /// </summary>
        [FieldName("Text")]
        public string Text { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="TextReplaceOperation"/> operation.
    /// </summary>
    public struct TextReplaceOperationOut
    {
        /// <summary>
        /// The resulting text after the replacements have been made.
        /// </summary>
        [FieldName("Text")]
        public string Text { get; set; }
    }

    /// <summary>
    /// Replaces a pattern in a text with a replacement.
    /// </summary>
    [OperationName(Display = "Text Replace", Type = "textReplace")]
    [OperationTag(OperationTags.Text)]
    public class TextReplaceOperation : TypedOperation
    <
        TextReplaceOperationIn,
        TextReplaceOperationOut
    >
    {
        /// <inheritdoc />
        public override async Task<TextReplaceOperationOut> Perform(TextReplaceOperationIn input)
        {
            Regex regex = input.Pattern.ToRegexPattern();
            return await Task.FromResult
            (
                new TextReplaceOperationOut() { Text = regex.Replace(input.Text, input.Replacement) }
            );
        }
    }
}