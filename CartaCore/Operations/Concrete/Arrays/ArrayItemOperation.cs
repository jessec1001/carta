using System;
using System.Threading.Tasks;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations.Arrays
{
    /// <summary>
    /// The input for the <see cref="ArrayItemOperation{TItem}" /> operation.
    /// </summary>
    /// <typeparam name="TItem">The type of array item.</typeparam>
    public struct ArrayItemOperationIn<TItem>
    {
        /// <summary>
        /// The array of items to index.
        /// </summary>
        [FieldName("Items")]
        public TItem[] Items { get; set; }
        /// <summary>
        /// The index of the item to obtain.
        /// </summary>
        [FieldName("Index")]
        public int Index { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="ArrayItemOperation{TItem}" /> operation.
    /// </summary>
    /// <typeparam name="TItem">The type of array item.</typeparam>
    public struct ArrayItemOperationOut<TItem>
    {
        /// <summary>
        /// The item that was indexed from the array.
        /// </summary>
        [FieldName("Item")]
        public TItem Item { get; set; }
    }

    /// <summary>
    /// Retrieves an item from an array located at a specified index.
    /// </summary>
    [OperationName(Display = "Array Item", Type = "arrayItem")]
    [OperationTag(OperationTags.Array)]
    public class ArrayItemOperation<TItem> : TypedOperation
    <
        ArrayItemOperationIn<TItem>,
        ArrayItemOperationOut<TItem>
    >
    {
        /// <inheritdoc />
        public override Task<ArrayItemOperationOut<TItem>> Perform(ArrayItemOperationIn<TItem> input)
        {
            // Check for a null array.
            if (input.Items is null)
                throw new ArgumentNullException($"Array of items was null.", nameof(input.Items));

            // Wrap around the index singly.
            if (input.Index < 0)
                input.Index += input.Items.Length;

            return Task.FromResult(new ArrayItemOperationOut<TItem>() { Item = input.Items[input.Index] });
        }
    }
}