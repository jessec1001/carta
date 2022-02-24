using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="OutputOperation{TValue}" /> operation.
    /// </summary>
    /// <typeparam name="TValue">The type of the output value.</typeparam>
    public struct OutputOperationIn<TValue>
    {
        /// <summary>
        /// The name of the output value.
        /// </summary>
        [FieldRequired]
        [FieldName("Name")]
        public string Name { get; set; }
        /// <summary>
        /// The value extracted from the output value to the external operation.
        /// </summary>
        [FieldName("Value")]
        public TValue Value { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="OutputOperation{TValue}" /> operation.
    /// </summary>
    public struct OutputOperationOut { }

    /// <summary>
    /// Specifies an expected output from the workflow that contains this operation.
    /// </summary>
    /// <typeparam name="TValue">The type of the output value.</typeparam>
    [OperationName(Display = "Output", Type = "workflowOutput")]
    [OperationTag(OperationTags.Workflow)]
    public class OutputOperation<TValue> : TypedOperation
    <
        OutputOperationIn<TValue>,
        OutputOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<OutputOperationOut> Perform(OutputOperationIn<TValue> input, OperationContext context)
        {
            OperationContext parentContext = context?.Parent;
            if (parentContext is null)
                throw new ArgumentNullException(nameof(context), "The output operation can not be executed independently.");
            if (!parentContext.Output.TryAdd(input.Name, input.Value))
                throw new KeyNotFoundException($"Output '{input.Name}' was already provided.");
            
            return Task.FromResult(new OutputOperationOut());
        }
    }
}