using System.Threading.Tasks;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="ArrayItemOperation" /> operation.
    /// </summary>
    public struct ArrayItemOperationIn
    {
        /// <summary>
        /// The array of items to index.
        /// </summary>
        public object[] Items { get; set; }
        /// <summary>
        /// The index of the item to obtain.
        /// </summary>
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
        public object Item { get; set; }
    }

    /// <summary>
    /// Retrieves an item from an array located at a specified index.
    /// </summary>
    public class ArrayItemOperation : TypedOperation
    <
        ArrayItemOperationIn,
        ArrayItemOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<ArrayItemOperationOut> Perform(ArrayItemOperationIn input)
        {
            return Task.FromResult(new ArrayItemOperationOut() { Item = input.Items[input.Index] });
        }
    }
}