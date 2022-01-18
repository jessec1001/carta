using System.Threading.Tasks;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    // TODO: Improve typing on operations such as this by using generics.
    /// <summary>
    /// The input for the <see cref="DelayOperation"/> operation.
    /// </summary>
    public struct DelayOperationIn
    {
        /// <summary>
        /// The amount of time to sleep.
        /// </summary>
        public int Milliseconds { get; set; }
        /// <summary>
        /// The value to pass along.
        /// </summary>
        public object Value { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="DelayOperation"/> operation.
    /// </summary>
    public struct DelayOperationOut
    {
        /// <summary>
        /// The value passed along.
        /// </summary>
        public object Value { get; set; }
    }

    /// <summary>
    /// Delays execution by a specified amount of time.
    /// </summary>
    [OperationName(Display = "Delay", Type = "delay")]
    [OperationTag(OperationTags.Workflow)]
    public class DelayOperation : TypedOperation
    <
        DelayOperationIn,
        DelayOperationOut
    >
    {
        /// <inheritdoc />
        public override async Task<DelayOperationOut> Perform(DelayOperationIn input)
        {
            await Task.Delay(input.Milliseconds);
            return new DelayOperationOut { Value = input.Value };
        }
    }
}