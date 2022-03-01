using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CartaCore.Extensions.Typing;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    // TODO: Consider configuring a type-optimized flag to indicate that we do not need to handle conversions.
    // TODO: Figure out how to handle the type conversion. We probably need a method to perform all conversions that
    //       relies on a type converter stack and relevant modifications.
    // TODO: Make this inherit from an operation template potentially.
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
        public TInput DefaultsTyped
        {
            get => Defaults.AsTyped<TInput>(DefaultTypeConverter);
            set => Defaults = value.AsDictionary();
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
            TInput input = job.Input.AsTyped<TInput>(DefaultTypeConverter);
            TOutput output = await Perform(input, job);
            job.Output = output.AsDictionary(DefaultTypeConverter);
        }

        /// <summary>
        /// Determines whether the operation is deterministic or non-deterministic on a specified job. This allows
        /// for operations to be memoized. By default, operations are assumed to be deterministic, and thus, memoized.
        /// </summary>
        /// <param name="input">The typed input to the operation.</param>
        /// <returns><c>true</c> if the operation is deterministic on a job; otherwise <c>false</c>.</returns>
        public virtual bool IsDeterministic(TInput input) => true;
        /// <inheritdoc />
        public override bool IsDeterministic(OperationJob job)
        {
            TInput input = job.Input.AsTyped<TInput>(DefaultTypeConverter);
            return IsDeterministic(input);
        }

        // TODO: We need to incorporate the attributes into the following methods.  
        /// <summary>
        /// Gets the input fields and their descriptors for this operation.
        /// </summary>
        /// <param name="input">The typed input to the operation.</param>
        /// <param name="job">The executing operation job.</param>
        /// <returns>An enumeration of input field descriptors.</returns>
        public virtual async IAsyncEnumerable<OperationFieldDescriptor> GetInputFields(TInput input, OperationJob job)
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
                        Schema = null
                    }
                );
            }
        }
        /// <inheritdoc />
        public override IAsyncEnumerable<OperationFieldDescriptor> GetInputFields(OperationJob job)
        {
            TInput input; // TODO: Implement.
            return GetInputFields(input, job);
        }
        /// <summary>
        /// Gets the output fields and their descriptors for this operation.
        /// </summary>
        /// <param name="output">The typed output to the operation.</param>
        /// <param name="job">The executing operation job.</param>
        /// <returns>An enumeration of output field descriptors.</returns>
        public virtual async IAsyncEnumerable<OperationFieldDescriptor> GetOutputFields(TOutput output, OperationJob job)
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
                        Schema = null
                    }
                );
            }
        } 
        /// <inheritdoc />
        public override IAsyncEnumerable<OperationFieldDescriptor> GetOutputFields(OperationJob job)
        {
            TOutput output; // TODO: Implement.
            return GetOutputFields(output, job);
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
        public override IAsyncEnumerable<OperationFieldDescriptor> GetExternalInputFields(OperationJob job)
        {
            TInput input; // TODO: Implement.
            return GetExternalInputFields(input, job);
        }
        /// <summary>
        /// Gets the output fields of the executing job and their descriptors for this operation.
        /// </summary>
        /// <param name="output">The typed output to the operation.</param>
        /// <param name="job">The executing operation job.</param>
        /// <returns>An enumeration of external output field descriptors.</returns>
        public virtual IAsyncEnumerable<OperationFieldDescriptor> GetExternalOutputFields(TOutput output, OperationJob job)
        {
            return Enumerable.Empty<OperationFieldDescriptor>().ToAsyncEnumerable();
        }
        /// <inheritdoc />
        public override IAsyncEnumerable<OperationFieldDescriptor> GetExternalOutputFields(OperationJob job)
        {
            TOutput output; // TODO: Implement.
            return GetExternalOutputFields(output, job);
        }

        // TODO: Check if there is a better/updated structure for dynamic fields.
        /// <summary>
        /// Gets the names of the dynamic input fields corresponding to particular input field.
        /// </summary>
        /// <param name="field">The input field to get the dynamic fields of.</param>
        /// <param name="input">The input being passed to the operation.</param>
        /// <returns>The dynamic input fields corresponding to a field.</returns>
        public virtual string[] GetDynamicInputFields(string field, TInput input) => Array.Empty<string>();
        /// <summary>
        /// Gets the names of the dynamic output fields corresponding to particular output field.
        /// </summary>
        /// <param name="field">The output field to get the dynamic fields of.</param>
        /// <param name="input">The input being passed to the operation.</param>
        /// <returns>The dynamic output fields corresponding to a field.</returns>
        public virtual string[] GetDynamicOutputFields(string field, TInput input) => Array.Empty<string>();

        // TODO: Abstract this into the `Operation` class.
        public OperationTemplate GetTemplate(TInput defaults)
        {
            // Get the operation name attribute assosicated with this operation.
            OperationNameAttribute attr = GetType().GetCustomAttribute<OperationNameAttribute>();
            if (attr is null) return null;
            else return new OperationTemplate()
            {
                Type = attr.Type,
                Subtype = null,
                Defaults = defaults.AsDictionary() // TODO: Take into consideration the type.
            };
        }
    }
}