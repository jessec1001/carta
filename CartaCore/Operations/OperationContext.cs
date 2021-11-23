using System.Collections.Generic;

namespace CartaCore.Operations
{
    /// <summary>
    /// A context that contains execution information for an operation.
    /// </summary>
    public class OperationContext
    {
        /// <summary>
        /// The parent context of this current context.
        /// </summary>
        public OperationContext Parent { get; init; }

        /// <summary>
        /// The input mapping for an operation.
        /// </summary>
        public Dictionary<string, object> Input { get; private init; }
        /// <summary>
        /// The output mapping for an operation.
        /// </summary>
        public Dictionary<string, object> Output { get; private init; }

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