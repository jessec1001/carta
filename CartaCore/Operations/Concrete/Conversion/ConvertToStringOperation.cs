using System.Threading.Tasks;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="ConvertToStringOperation"/> operation.
    /// </summary>
    public struct ConvertToStringOperationIn
    {
        /// <summary>
        /// The number to convert to a string.
        /// </summary>
        public double Number { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="ConvertToStringOperation"/> operation.
    /// </summary>
    public struct ConvertToStringOperationOut
    {
        /// <summary>
        /// The string representation.
        /// </summary>
        public string Text { get; set; }
    }

    /// <summary>
    /// Formats a number into a text string.
    /// </summary>
    [OperationName(Display = "Convert to String", Type = "convertToString")]
    [OperationTag(OperationTags.Conversion)]
    public class ConvertToStringOperation : TypedOperation
    <
        ConvertToStringOperationIn,
        ConvertToStringOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<ConvertToStringOperationOut> Perform(ConvertToStringOperationIn input)
        {
            return Task.FromResult(new ConvertToStringOperationOut() { Text = input.Number.ToString() });
        }
    }
}