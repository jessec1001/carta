using System;
using System.Threading.Tasks;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations.Conversion
{
    /// <summary>
    /// The input for the <see cref="ConvertToNumberOperation"/> operation.
    /// </summary>
    public struct ConvertToNumberOperationIn
    {
        /// <summary>
        /// The text to convert to a number.
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// The default value to use if the conversion fails. 
        /// </summary>
        public double? Default { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="ConvertToNumberOperation"/> operation.
    /// </summary>
    public struct ConvertToNumberOperationOut
    {
        /// <summary>
        /// The parsed number.
        /// </summary>
        public double Number { get; set; }
    }

    /// <summary>
    /// Parses a text string into a number. 
    /// </summary>
    [OperationName(Display = "Convert to Number", Type = "convertToNumber")]
    [OperationTag(OperationTags.Conversion)]
    public class ConvertToNumberOperation : TypedOperation
    <
        ConvertToNumberOperationIn,
        ConvertToNumberOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<ConvertToNumberOperationOut> Perform(ConvertToNumberOperationIn input)
        {
            if (double.TryParse(input.Text, out double number))
                return Task.FromResult(new ConvertToNumberOperationOut() { Number = number });
            else if (input.Default.HasValue)
                return Task.FromResult(new ConvertToNumberOperationOut() { Number = input.Default.Value });
            else
                throw new ArgumentException("Failed to parse string into number.");
        }
    }
}