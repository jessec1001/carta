using System.Threading.Tasks;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations.Text
{
    /// <summary>
    /// The input for the <see cref="TextLengthOperation"/> operation.
    /// </summary>
    public struct TextLengthOperationIn
    {
        /// <summary>
        /// The text to measure.
        /// </summary>
        [FieldName("Text")]
        public string Text { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="TextLengthOperation"/> operation.
    /// </summary>
    public struct TextLengthOperationOut
    {
        /// <summary>
        /// The length of the text.
        /// </summary>
        [FieldName("Length")]
        public int Length { get; set; }
    }

    /// <summary>
    /// Measures the length of some text in characters.
    /// </summary>
    [OperationName(Display = "Text Length", Type = "textLength")]
    [OperationTag(OperationTags.Text)]
    public class TextLengthOperation : TypedOperation
    <
        TextLengthOperationIn,
        TextLengthOperationOut
    >
    {
        /// <inheritdoc />
        public override async Task<TextLengthOperationOut> Perform(TextLengthOperationIn input)
        {
            return await Task.FromResult(new TextLengthOperationOut() { Length = input.Text.Length });
        }
    }
}