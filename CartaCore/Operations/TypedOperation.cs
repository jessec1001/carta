using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CartaCore.Extensions.Typing;
using CartaCore.Typing;

namespace CartaCore.Operations
{
    /// <summary>
    /// Represents an abstract operation that takes a particular typed input and produces a particular typed output.
    /// </summary>
    /// <typeparam name="TInput">The input type.</typeparam>
    /// <typeparam name="TOutput">The output type.</typeparam>
    public abstract class TypedOperation<TInput, TOutput> : Operation
        where TInput : new()
        where TOutput : new()
    {
        /// <summary>
        /// The typed default values of the operation as specified externally.
        /// </summary>
        public async Task<TInput> DefaultsTyped(OperationJob job = null)
        {
            return await ConvertInput(job, Defaults);
        }

        /// <summary>
        /// The default type converter stack used for converting input and output values to the correct type.w
        /// </summary>
        /// <returns></returns>
        protected static readonly TypeConverterContext TypeConverter = new(
            new EnumTypeConverter(),
            new NumericTypeConverter(),
            new ArrayTypeConverter(),
            new DictionaryTypeConverter()
        );

        /// <summary>
        /// Merges a dictionary of values with a dictionary of default values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="defaults">The default values.</param>
        /// <returns>The merged values.</returns>
        public IDictionary<string, object> MergeDefaults(
            IDictionary<string, object> values,
            IDictionary<string, object> defaults = null)
        {
            IDictionary<string, object> merged = new Dictionary<string, object>(values);
            if (defaults is not null)
            {
                foreach (KeyValuePair<string, object> pair in defaults)
                {
                    if (!merged.ContainsKey(pair.Key))
                        merged[pair.Key] = pair.Value;
                }
            }
            return merged;
        }

        /// <summary>
        /// Converts a particular input object into the expected type for the corresponding input field.
        /// The only input fields that are considered are those that are returned from <see cref="GetInputFields(OperationJob)"/>.
        /// </summary>
        /// <param name="field">The description of the field.</param>
        /// <param name="input">The input object.</param>
        /// <param name="job">The job performing the operation.</param>
        /// <returns>The converted field value.</returns>
        public virtual Task<object> ConvertInputField(
            OperationFieldDescriptor field,
            object input,
            OperationJob job)
        {
            if (TypeConverter.TryConvert(input?.GetType() ?? typeof(object), field.Type, in input, out object converted))
                return Task.FromResult(converted);
            else
            {
                throw new InvalidCastException(
                    $"Could not convert input field '{field.Name}' to expected type of '{field.Type.Name}'."
                );
            }
        }
        /// <summary>
        /// Converts the input dictionary for a job into a typed input.
        /// </summary>
        /// <param name="job">The job performing the operation.</param>
        /// <param name="inputs">The input dictionary.</param>
        /// <returns>The typed input.</returns>
        public virtual async Task<TInput> ConvertInput(OperationJob job, IDictionary<string, object> inputs)
        {
            // TODO: This is making an infinite recursive call to `GetInputFields` and back to `ConvertInput`.
            // For each input field, try to perform a convert and set the corresponding property.
            object typedInput = new TInput();
            await foreach (OperationFieldDescriptor field in GetInputFields(job))
            {
                // Try to find the corresponding property.
                PropertyInfo property = typeof(TInput).GetProperty(
                    field.Name,
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    BindingFlags.IgnoreCase
                );
                if (property is null) continue;

                // Set the property value after conversion.
                if (inputs.TryGetValue(field.Name, out object input))
                {
                    object converted = await ConvertInputField(field, input, job);
                    property.SetValue(typedInput, converted);
                }
            }
            return (TInput)typedInput;
        }
        /// <summary>
        /// Converts a particular output object into the expected type for the corresponding output field.
        /// The only output fields that are considered are those that are returned from <see cref="GetOutputFields(OperationJob)"/>.
        /// </summary>
        /// <param name="field">The description of the field.</param>
        /// <param name="output">The output object.</param>
        /// <param name="job">The job performing the operation.</param>
        /// <returns>The converted field value.</returns>
        public virtual Task<object> ConvertOutputField(
            OperationFieldDescriptor field,
            object output,
            OperationJob job)
        {
            if (TypeConverter.TryConvert(output?.GetType() ?? typeof(object), field.Type, in output, out object converted))
                return Task.FromResult(converted);
            else
            {
                throw new InvalidCastException(
                    $"Could not convert output field '{field.Name}' to expected type of '{field.Type.Name}'."
                );
            }
        }
        /// <summary>
        /// Converts the output values for a job into a dictionary.
        /// </summary>
        /// <param name="job">The job performing the operation.</param>
        /// <param name="outputs">The output values.</param>
        /// <returns>The dictionary output.</returns>
        public virtual async Task<IDictionary<string, object>> ConvertOutput(OperationJob job, TOutput outputs)
        {
            // For each output field, try to perform a convert and set the corresponding property.
            IDictionary<string, object> dictOutput = new Dictionary<string, object>();
            await foreach (OperationFieldDescriptor field in GetOutputFields(job))
            {
                // Try to find the corresponding property.
                PropertyInfo property = typeof(TOutput).GetProperty(
                    field.Name,
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    BindingFlags.IgnoreCase
                );
                if (property is null) continue;

                // Append the property value after conversion.
                object output = property.GetValue(outputs);
                object converted = await ConvertOutputField(field, output, job);
                dictOutput[field.Name] = converted;
            }
            return dictOutput;
        }

        /// <summary>
        /// Operates on a specified operation job containing input and output mappings. Most operations will use the
        /// input mapping to produce an output mapping. This method is implemented by concrete subclasses.
        /// </summary>
        /// <param name="input">The typed input to the operation.</param>
        /// <returns>The typed output from the operation.</returns>
        public virtual Task<TOutput> Perform(TInput input) => Task.FromResult(default(TOutput));
        /// <summary>
        /// Operates on a specified operation job containing input and output mappings. Most operations will use the
        /// input mapping to produce an output mapping. This method is implemented by concrete subclasses.
        /// </summary>
        /// <param name="input">The typed input to the operation.</param>
        /// <param name="job">
        /// This is a reference to the operation job that contains the input and output mappings. Additionally,
        /// ff the operation is being called from within a workflow, this is a reference to the workflow job.
        /// </param>
        /// <returns>The typed output from the operation.</returns>
        public virtual Task<TOutput> Perform(TInput input, OperationJob job = null) => Perform(input);
        /// <inheritdoc />
        public override async Task Perform(OperationJob job)
        {
            TInput input = await ConvertInput(job, MergeDefaults(job.Input, Defaults));
            TOutput output = await Perform(input, job);
            foreach (KeyValuePair<string, object> entry in await ConvertOutput(job, output))
                job.Output.TryAdd(entry.Key, entry.Value);
        }

        /// <summary>
        /// Determines whether the operation is deterministic or non-deterministic on a specified job. This allows
        /// for operations to be memoized. By default, operations are assumed to be deterministic, and thus, memoized.
        /// </summary>
        /// <param name="input">The typed input to the operation.</param>
        /// <returns><c>true</c> if the operation is deterministic on a job; otherwise <c>false</c>.</returns>
        public virtual Task<bool> IsDeterministic(TInput input) => Task.FromResult(true);
        /// <inheritdoc />
        public override async Task<bool> IsDeterministic(OperationJob job)
        {
            TInput input = await ConvertInput(job, MergeDefaults(job.Input, Defaults));
            return await IsDeterministic(input);
        }

        // TODO: We need to incorporate the attributes into the following methods.  
        /// <inheritdoc />
        public override async IAsyncEnumerable<OperationFieldDescriptor> GetInputFields(OperationJob job)
        {
            // Get the public properties by ignoring case for web portability.
            PropertyInfo[] properties = typeof(TInput)
                .GetProperties(
                    BindingFlags.IgnoreCase |
                    BindingFlags.Instance |
                    BindingFlags.Public
                );

            // Construct each of the field descriptors from the known properties.
            foreach (PropertyInfo property in properties)
            {
                yield return await Task.FromResult(
                    new OperationFieldDescriptor
                    {
                        Name = property.Name,
                        Type = property.PropertyType,
                        Attributes = property.GetCustomAttributes().ToList(),
                        Schema = OperationHelper.GenerateSchema(property)
                    }
                );
            }
        }
        /// <inheritdoc />
        public override async IAsyncEnumerable<OperationFieldDescriptor> GetOutputFields(OperationJob job)
        {
            // Get the public properties by ignoring case for web portability.
            PropertyInfo[] properties = typeof(TOutput)
                .GetProperties(
                    BindingFlags.IgnoreCase |
                    BindingFlags.Instance |
                    BindingFlags.Public
                );

            // Construct each of the field descriptors from the known properties.
            foreach (PropertyInfo property in properties)
            {
                yield return await Task.FromResult(
                    new OperationFieldDescriptor
                    {
                        Name = property.Name,
                        Type = property.PropertyType,
                        Attributes = property.GetCustomAttributes().ToList(),
                        Schema = OperationHelper.GenerateSchema(property)
                    }
                );
            }
        }

        /// <summary>
        /// Gets the input fields of the executing job and their descriptors for this operation.
        /// </summary>
        /// <param name="input">The typed input to the operation.</param>
        /// <param name="job">The executing operation job.</param>
        /// <returns>An enumeration of external input field descriptors.</returns>
        public virtual IAsyncEnumerable<OperationFieldDescriptor> GetExternalInputFields(TInput input, OperationJob job)
        {
            return Enumerable.Empty<OperationFieldDescriptor>().ToAsyncEnumerable();
        }
        /// <inheritdoc />
        public override async IAsyncEnumerable<OperationFieldDescriptor> GetExternalInputFields(OperationJob job)
        {
            TInput input = await ConvertInput(job, MergeDefaults(job.Input, Defaults));
            await foreach (OperationFieldDescriptor field in GetExternalInputFields(input, job))
                yield return field;
        }
        /// <summary>
        /// Gets the output fields of the executing job and their descriptors for this operation.
        /// </summary>
        /// <param name="input">The typed input to the operation.</param>
        /// <param name="job">The executing operation job.</param>
        /// <returns>An enumeration of external output field descriptors.</returns>
        public virtual IAsyncEnumerable<OperationFieldDescriptor> GetExternalOutputFields(TInput input, OperationJob job)
        {
            return Enumerable.Empty<OperationFieldDescriptor>().ToAsyncEnumerable();
        }
        /// <inheritdoc />
        public override async IAsyncEnumerable<OperationFieldDescriptor> GetExternalOutputFields(OperationJob job)
        {
            TInput input = await ConvertInput(job, MergeDefaults(job.Input, Defaults));
            await foreach (OperationFieldDescriptor field in GetExternalOutputFields(input, job))
                yield return field;
        }

        /// <summary>
        /// Gets the template information associated with this operation.
        /// This information contains precisely enough to instantiate an operation without knowing the type.
        /// </summary>
        /// <param name="defaults">The default values for the operation. Optional.</param>
        /// <returns>The operation template.</returns>
        public OperationTemplate GetTemplate(TInput defaults)
        {
            IDictionary<string, object> untypedDefaults = defaults.AsDictionary(TypeConverter);
            return GetTemplate(untypedDefaults);
        }
    }
}