using System.Threading.Tasks;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="DelayOperation{TValue}"/> operation.
    /// </summary>
    public struct DelayOperationIn<TValue>
    {
        /// <summary>
        /// The value to pass along.
        /// </summary>
        [FieldPipelined]
        [FieldName("Value")]
        public TValue Value { get; set; }
        /// <summary>
        /// The amount of time to delay in milliseconds.
        /// </summary>
        [FieldName("Delay")]
        public int Delay { get; set; }
        // TODO: Display this only when the type of value is enumerable.
        /// <summary>
        /// Whether there should be a delay between each item in the value.
        /// </summary>
        [FieldName("Per Item")]
        public bool PerItem { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="DelayOperation{TValue}"/> operation.
    /// </summary>
    public struct DelayOperationOut<TValue>
    {
        /// <summary>
        /// The value passed along.
        /// </summary>
        [FieldName("Value")]
        public TValue Value { get; set; }
    }

    /// <summary>
    /// Delays execution by a specified amount of time.
    /// </summary>
    [OperationName(Display = "Delay", Type = "delay")]
    [OperationTag(OperationTags.Workflow)]
    public class DelayOperation<TValue> : TypedOperation
    <
        DelayOperationIn<TValue>,
        DelayOperationOut<TValue>
    >
    {
        /// <inheritdoc />
        public override async Task<DelayOperationOut<TValue>> Perform(DelayOperationIn<TValue> input)
        {
            await Task.Delay(input.Delay);
            return new DelayOperationOut<TValue> { Value = input.Value };
        }
    }
}