using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Documentation;
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
        /// The description of the input value.
        /// This is displayed to users of the containing workflow. 
        /// </summary>
        [FieldName("Description")]
        public string Description { get; set; }
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
        public override Task<InputOperationOut<TValue>> Perform(InputOperationIn input, OperationJob job)
        {
            // Notice that if the value is not required, it will default to the default value of the type.
            OperationJob parentJob = job?.Parent;
            if (parentJob is null)
                throw new ArgumentNullException(nameof(job), "The input operation can not be executed independently.");
            if (!parentJob.Input.TryGetValue(input.Name, out object value))
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

        // TODO: We need a typed version of this method.
        /// <inheritdoc />
        public override async IAsyncEnumerable<OperationFieldDescriptor> GetExternalInputFields(
            InputOperationIn input,
            OperationJob job)
        {
            // Construct the relevant attributes for the input value.
            List<Attribute> attributes = new();
            if (input.Required) attributes.Add(new FieldRequiredAttribute());

            // Construct the relevant documentation for the input value.
            StandardDocumentation documentation = new() { Summary = input.Description };

            // Construct and yield the field descriptor for the external field.
            yield return await Task.FromResult(new OperationFieldDescriptor
            {
                Name = input.Name,
                Type = typeof(TValue),
                Documentation = documentation,
                Schema = null, // TODO: Add a method that helps generate schema.
                Attributes = attributes
            });
        }
    }
}