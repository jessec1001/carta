using System;
using System.Threading.Tasks;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="OutputOperation" /> operation.
    /// </summary>
    public struct OutputOperationIn
    {
        /// <summary>
        /// The name of the output value.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The value extracted from the output value to the external operation.
        /// </summary>
        public object Value { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="OutputOperation" /> operation.
    /// </summary>
    public struct OutputOperationOut { }

    /// <summary>
    /// Specifies an expected output from the workflow that contains this operation.
    /// </summary>
    [OperationName(Display = "Output", Type = "workflowOutput")]
    [OperationTag(OperationTags.Workflow)]
    public class OutputOperation : TypedOperation
    <
        OutputOperationIn,
        OutputOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<OutputOperationOut> Perform(OutputOperationIn input, OperationContext context)
        {
            OperationContext parentContext = context?.Parent;
            if (parentContext is not null && parentContext.Output.TryAdd(input.Name, input.Value))
                return Task.FromResult(new OutputOperationOut());
            else
                throw new ArgumentException($"Workflow output '{input.Name}' was duplicated or has an invalid name.");
        }
    }
}