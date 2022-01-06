using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CartaCore.Operations.Attributes;
using CartaCore.Utilities;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="TextSplitOperation"/> operation.
    /// </summary>
    public struct TextSplitOperationIn
    {
        /// <summary>
        /// The pattern to split the text by. This can be plain text or a regular expression.
        /// - If splitting by plain text, just enter the text you want to split by.
        /// - If splitting by regular expression, enter your regular expression surrounded by forward slashes like "/[0-9a-f]+/".
        /// </summary>
        public string Pattern { get; set; }
        /// <summary>
        /// The text to split.
        /// </summary>
        public string Text { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="TextSplitOperation"/> operation.
    /// </summary>
    public struct TextSplitOperationOut
    {
        /// <summary>
        /// The resulting list of parts that the text was split into.
        /// </summary>
        public string[] Parts { get; set; }
    }

    /// <summary>
    /// Splits a string into parts based on a pattern.
    /// </summary>
    [OperationName(Display = "Text Split", Type = "textSplit")]
    [OperationTag(OperationTags.Text)]
    public class TextSplitOperation : TypedOperation
    <
        TextSplitOperationIn,
        TextSplitOperationOut
    >
    {
        /// <inheritdoc />
        public override async Task<TextSplitOperationOut> Perform(TextSplitOperationIn input)
        {
            Regex regex = input.Pattern.ToRegexPattern();
            return await Task.FromResult(new TextSplitOperationOut() { Parts = regex.Split(input.Text) });
        }
    }
}