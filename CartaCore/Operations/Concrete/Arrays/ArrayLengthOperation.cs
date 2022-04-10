using System.Threading.Tasks;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations.Arrays
{
    /// <summary>
    /// The input for the <see cref="ArrayLengthOperation" /> operation.
    /// </summary>
    public struct ArrayLengthOperationIn
    {
        /// <summary>
        /// The array of items to count.
        /// </summary>
        [FieldName("Items")]
        public object[] Items { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="ArrayLengthOperation" /> operation.
    /// </summary>
    public struct ArrayLengthOperationOut
    {
        /// <summary>
        /// The number of items in the array.
        /// </summary>
        [FieldName("Length")]
        public int Length { get; set; }
    }

    /// <summary>
    /// Counts the number of items in an array.
    /// </summary>
    [OperationName(Display = "Array Length", Type = "arrayLength")]
    [OperationTag(OperationTags.Array)]
    [OperationHidden]
    public class ArrayLengthOperation : TypedOperation
    <
        ArrayLengthOperationIn,
        ArrayLengthOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<ArrayLengthOperationOut> Perform(ArrayLengthOperationIn input)
        {
            return Task.FromResult(new ArrayLengthOperationOut() { Length = input.Items?.Length ?? 0 });
        }
    }
}