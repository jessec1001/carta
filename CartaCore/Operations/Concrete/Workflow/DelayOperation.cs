using System;
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
        [FieldName("Value")]
        public TValue Value { get; set; }
        /// <summary>
        /// The amount of time to delay in milliseconds.
        /// </summary>
        [FieldDefault(1000)]
        [FieldRange(Minimum = 0, Maximum = 60000)]
        [FieldName("Delay")]
        public int Delay { get; set; }
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
    [OperationHidden]
    public class DelayOperation<TValue> : TypedOperation
    <
        DelayOperationIn<TValue>,
        DelayOperationOut<TValue>
    >
    {
        /// <inheritdoc />
        public override async Task<DelayOperationOut<TValue>> Perform(DelayOperationIn<TValue> input)
        {
            // Clamp the delay to a reasonable range.
            int delay = Math.Max(Math.Min(input.Delay, 60000), 0);

            await Task.Delay(delay);
            return new DelayOperationOut<TValue> { Value = input.Value };
        }
    }
}