using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CartaCore.Operations
{
    /// <summary>
    /// A context that contains execution information for an operation.
    /// </summary>
    public class OperationContext
    {
        // TODO: We need to be able to hash and compare selectors so that they are not duplicated on the queue.
        // Create a queue to store selectors based on priority.
        public ConcurrentQueue<(Selector, object)> Selectors { get; } = new();

        /// <summary>
        /// The parent context of this current context.
        /// </summary>
        public OperationContext Parent { get; init; }
        /// <summary>
        /// A reference to the executing operation.
        /// </summary>
        public Operation Operation { get; init; }

        /// <summary>
        /// The number of threads that are available to the operation.
        /// </summary>
        public int Threads { get; set; }

        // TODO: Check if this default dictionary is redundant.
        /// <summary>
        /// The default for the operation.
        /// </summary>
        public Dictionary<string, object> Default { get; set; }
        /// <summary>
        /// The input mapping for an operation.
        /// </summary>
        public Dictionary<string, object> Input { get; set; }
        /// <summary>
        /// The output mapping for an operation.
        /// </summary>
        public Dictionary<string, object> Output { get; set; }

        /// <summary>
        /// The total combined input and default to the operation.
        /// </summary>
        public Dictionary<string, object> Total
        {
            get
            {
                Dictionary<string, object> total = new();
                foreach (KeyValuePair<string, object> entry in Input)
                    total.TryAdd(entry.Key, entry.Value);
                foreach (KeyValuePair<string, object> entry in Default)
                    total.TryAdd(entry.Key, entry.Value);
                return total;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationContext"/> class with optionally specified mappings
        /// for inputs and outputs.
        /// </summary>
        /// <param name="input">The input mapping.</param>
        /// <param name="output">The output mapping.</param>
        public OperationContext(
            Dictionary<string, object> input = null,
            Dictionary<string, object> output = null
        )
        {
            Input = input ?? new Dictionary<string, object>();
            Output = output ?? new Dictionary<string, object>();
        }
    }
}