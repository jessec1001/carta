using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="DelayItemsOperation{TValue}"/> operation.
    /// </summary>
    public struct DelayItemsOperationIn<TValue>
    {
        /// <summary>
        /// The values to pass along.
        /// </summary>
        [FieldCorrelation]
        [FieldName("Values")]
        public IAsyncEnumerable<TValue> Values { get; set; }
        /// <summary>
        /// The amount of time to delay in milliseconds.
        /// This delay is applied per-item.
        /// </summary>
        [FieldDefault(1000)]
        [FieldRange(Minimum = 0, Maximum = 60000)]
        [FieldName("Delay")]
        public int Delay { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="DelayItemsOperation{TValue}"/> operation.
    /// </summary>
    public struct DelayItemsOperationOut<TValue>
    {
        /// <summary>
        /// The values passed along.
        /// </summary>
        [FieldName("Values")]
        public IAsyncEnumerable<TValue> Values { get; set; }
    }

    /// <summary>
    /// Delays execution over a set of values by a specified amount of time.
    /// </summary>
    /// <typeparam name="TValue">The type of item.</typeparam>
    [OperationName(Display = "Delay Items", Type = "delayItems")]
    [OperationTag(OperationTags.Workflow)]
    [OperationHidden]
    public class DelayItemsOperation<TValue> : TypedOperation
    <
        DelayItemsOperationIn<TValue>,
        DelayItemsOperationOut<TValue>
    >
    {
        private static async IAsyncEnumerable<TValue> DelayItems(IAsyncEnumerable<TValue> values, int delay)
        {
            await foreach (var value in values)
            {
                await Task.Delay(delay);
                yield return value;
            }
        }

        /// <inheritdoc />
        public override Task<DelayItemsOperationOut<TValue>> Perform(DelayItemsOperationIn<TValue> input)
        {
            // Clamp the delay to a reasonable range.
            int delay = Math.Max(Math.Min(input.Delay, 60000), 0);

            return Task.FromResult(new DelayItemsOperationOut<TValue>
            {
                Values = DelayItems(input.Values, delay)
            });
        }
    }
}