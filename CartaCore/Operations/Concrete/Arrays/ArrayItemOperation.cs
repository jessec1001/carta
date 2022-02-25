using System;
using System.Threading.Tasks;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations.Arrays
{
    // TODO: Make this a generic operation that can be used for any array type.
    /// <summary>
    /// The input for the <see cref="ArrayItemOperation" /> operation.
    /// </summary>
    public struct ArrayItemOperationIn
    {
        /// <summary>
        /// The array of items to index.
        /// </summary>
        [FieldName("Items")]
        public object[] Items { get; set; }
        /// <summary>
        /// The index of the item to obtain.
        /// </summary>
        [FieldName("Index")]
        public int Index { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="ArrayItemOperation" /> operation.
    /// </summary>
    public struct ArrayItemOperationOut
    {
        /// <summary>
        /// The item that was indexed from the array.
        /// </summary>
        [FieldName("Item")]
        public object Item { get; set; }
    }

    /// <summary>
    /// Retrieves an item from an array located at a specified index.
    /// </summary>
    [OperationName(Display = "Array Item", Type = "arrayItem")]
    [OperationTag(OperationTags.Array)]
    public class ArrayItemOperation : TypedOperation
    <
        ArrayItemOperationIn,
        ArrayItemOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<ArrayItemOperationOut> Perform(ArrayItemOperationIn input)
        {
            // Check for a null array.
            if (input.Items is null)
                throw new ArgumentNullException($"Array of items was null.", nameof(input.Items));

            // Wrap around the index singly.
            if (input.Index < 0)
                input.Index += input.Items.Length;

            return Task.FromResult(new ArrayItemOperationOut() { Item = input.Items[input.Index] });
        }
    }
}