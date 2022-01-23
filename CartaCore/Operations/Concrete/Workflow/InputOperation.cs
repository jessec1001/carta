using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="InputOperation" /> operation.
    /// </summary>
    public struct InputOperationIn
    {
        /// <summary>
        /// The name of the input value.
        /// </summary>
        public string Name { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="InputOperation" /> operation.
    /// </summary>
    public struct InputOperationOut
    {
        /// <summary>
        /// The value assigned to the input value from the external operation.
        /// </summary>
        public object Value { get; set; }
    }

    /// <summary>
    /// Specifies an expected input to the workflow that contains this operation.
    /// </summary>
    [OperationName(Display = "Input", Type = "workflowInput")]
    [OperationTag(OperationTags.Workflow)]
    public class InputOperation : TypedOperation
    <
        InputOperationIn,
        InputOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<InputOperationOut> Perform(InputOperationIn input, OperationContext context)
        {
            OperationContext parentContext = context?.Parent;
            if (parentContext is not null && parentContext.Input.TryGetValue(input.Name, out object value))
                return Task.FromResult(new InputOperationOut { Value = value });
            else
                throw new KeyNotFoundException($"Workflow input '{input.Name}' was expected but not provided.");
        }
    }
}