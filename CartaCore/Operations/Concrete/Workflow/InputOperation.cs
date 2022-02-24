using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="InputOperation{TValue}" /> operation.
    /// </summary>
    public struct InputOperationIn
    {
        /// <summary>
        /// The name of the input value.
        /// </summary>
        [FieldRequired]
        [FieldName("Name")]
        public string Name { get; set; }
        /// <summary>
        /// Whether the input value is required.
        /// </summary>
        [FieldName("Required")]
        [FieldDefault(false)]
        public bool Required { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="InputOperation{TValue}" /> operation.
    /// </summary>
    /// <typeparam name="TValue">The type of the input value.</typeparam>
    public struct InputOperationOut<TValue>
    {
        /// <summary>
        /// The value assigned to the input value from the external operation.
        /// </summary>
        [FieldName("Value")]
        public TValue Value { get; set; }
    }

    /// <summary>
    /// Specifies an expected input to the workflow that contains this operation.
    /// </summary>
    /// <typeparam name="TValue">The type of the input value.</typeparam>
    [OperationName(Display = "Input", Type = "workflowInput")]
    [OperationTag(OperationTags.Workflow)]
    public class InputOperation<TValue> : TypedOperation
    <
        InputOperationIn,
        InputOperationOut<TValue>
    >
    {
        /// <inheritdoc />
        public override Task<InputOperationOut<TValue>> Perform(InputOperationIn input, OperationContext context)
        {
            // Notice that if the value is not required, it will default to the default value of the type.
            OperationContext parentContext = context?.Parent;
            if (parentContext is null)
                throw new ArgumentNullException(nameof(context), "The input operation can not be executed independently.");
            if (!parentContext.Input.TryGetValue(input.Name, out object value))
            {
                if (input.Required)
                    throw new KeyNotFoundException($"Input '{input.Name}' was required but not provided.");
                else
                    value = default(TValue);
            }
            if (value is not TValue typedValue)
                throw new InvalidCastException();

            return Task.FromResult(new InputOperationOut<TValue> { Value = typedValue });
        }
    }
}