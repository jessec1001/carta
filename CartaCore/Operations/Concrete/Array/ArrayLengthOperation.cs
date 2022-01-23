using System.Threading.Tasks;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations.Array
{
    /// <summary>
    /// The input for the <see cref="ArrayLengthOperation" /> operation.
    /// </summary>
    public struct ArrayLengthOperationIn
    {
        /// <summary>
        /// The array of items to count.
        /// </summary>
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
        public int Count { get; set; }
    }

    /// <summary>
    /// Counts the number of items in an array.
    /// </summary>
    [OperationName(Display = "Array Length", Type = "arrayLength")]
    [OperationTag(OperationTags.Array)]
    public class ArrayLengthOperation : TypedOperation
    <
        ArrayLengthOperationIn,
        ArrayLengthOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<ArrayLengthOperationOut> Perform(ArrayLengthOperationIn input)
        {
            return Task.FromResult(new ArrayLengthOperationOut() { Count = input.Items.Length });
        }
    }
}