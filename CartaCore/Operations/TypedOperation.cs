using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CartaCore.Extensions.Typing;
using CartaCore.Operations.Attributes;

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
        // TODO: This needs to be updated to reflect custom conversion in derived classes.
        /// <summary>
        /// The typed default values of the operation as specified externally.
        /// </summary>
        public TInput DefaultsTyped
        {
            set => Defaults = value.AsDictionary();
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
                // We iterate over any overriding conversions and apply them in sequence.
                object converted = null;
                if (inputs.TryGetValue(field.Name, out object input))
                    converted = input;

                foreach (Attribute attribute in field.Attributes)
                {
                    if (attribute is IOverrideConversionAttribute overrideConversion)
                        converted = await overrideConversion.ConvertInputField(this, field, converted, job);
                }
                converted = await ConvertInputField(field, converted, job);
                property.SetValue(typedInput, converted);
            }
            return (TInput)typedInput;
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
                // We iterate over any overriding conversions and apply them in sequence.
                object output = property.GetValue(outputs);
                object converted = output;
                foreach (Attribute attribute in property.PropertyType.GetCustomAttributes())
                {
                    if (attribute is IOverrideConversionAttribute conversionAttribute)
                        converted = await conversionAttribute.ConvertOutputField(this, field, converted, job);
                }
                converted = await ConvertOutputField(field, converted, job);
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
            // Setup the job status to be started.
            OperationStatus status = new()
            {
                ParentId = job?.Parent?.Operation?.Id,
                OperationId = job.Operation.Id,
                Started = true
            };
            job.Status.TryAdd(Id, status);
            await job.OnUpdate(job);

            // Try to perform the operation.
            try
            {
                TInput input = await ConvertInput(job, MergeDefaults(job.Input, Defaults));
                TOutput output = await Perform(input, job);
                foreach (KeyValuePair<string, object> entry in await ConvertOutput(job, output))
                    job.Output.TryAdd(entry.Key, entry.Value);
            }
            catch (Exception ex)
            {
                // Update the status with the exception.
                job.Status.TryGetValue(Id, out status);
                job.Status.TryUpdate(Id, status with
                {
                    Finished = true,
                    Exception = ex
                }, status);

                throw;
            }
            finally
            {
                // Update the status to be finished.
                job.Status.TryGetValue(Id, out status);
                job.Status.TryUpdate(Id, status with
                {
                    Finished = true
                }, status);
            }

            // Notify the job that the operation has finished.
            await job.OnUpdate(job);
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

        /// <inheritdoc />
        public override IAsyncEnumerable<OperationFieldDescriptor> GetExternalInputFields(OperationJob job)
        {
            return Enumerable.Empty<OperationFieldDescriptor>().ToAsyncEnumerable();
        }
        /// <inheritdoc />
        public override IAsyncEnumerable<OperationFieldDescriptor> GetExternalOutputFields(OperationJob job)
        {
            return Enumerable.Empty<OperationFieldDescriptor>().ToAsyncEnumerable();
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