using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Documentation;
using CartaCore.Operations.Attributes;
using NJsonSchema;

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
        /// The description of the output value.
        /// This is displayed to users of the containing workflow.
        /// </summary>
        [FieldName("Description")]
        public string Description { get; set; }
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
        public override Task<OutputOperationOut> Perform(OutputOperationIn<TValue> input, OperationJob job)
        {
            OperationJob parentJob = job?.Parent;
            if (input.Name is null)
                throw new ArgumentNullException(nameof(input.Name), "The name of the output value must be specified.");
            if (parentJob is null)
                throw new ArgumentNullException(nameof(job), "The output operation can not be executed independently.");
            if (!parentJob.Output.TryAdd(input.Name, input.Value))
                throw new KeyNotFoundException($"Output '{input.Name}' was already provided.");

            return Task.FromResult(new OutputOperationOut());
        }

        /// <inheritdoc />
        public override async IAsyncEnumerable<OperationFieldDescriptor> GetExternalOutputFields(
            OutputOperationIn<TValue> input,
            OperationJob job)
        {
            // If the output name is not specified, then we can't provide any external fields.
            if (input.Name is null)
                yield break;

            // Construct the relevant attributes for the output value.
            List<Attribute> attributes = new();

            // Construct the JSON schema for the output value.
            JsonSchema schema = OperationHelper.GenerateSchema(typeof(TValue), attributes);
            schema.Title = input.Name;
            schema.Description = input.Description;

            // Construct and yield the field descriptor for the external field.
            yield return await Task.FromResult(new OperationFieldDescriptor
            {
                Name = input.Name,
                Type = typeof(TValue),
                Schema = schema,
                Attributes = attributes
            });
        }
    }
}