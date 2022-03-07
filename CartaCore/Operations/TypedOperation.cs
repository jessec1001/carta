using System;
using System.Collections.Concurrent;
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
        public TInput DefaultsTyped
        {
            get => ConvertToTyped<TInput>(Defaults);
            set => Defaults = value.AsDictionary();
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

        // TODO: This conversion should happen field-by-field so as to catch type errors at the field level.
        /// <summary>
        /// Converts the specified dictionary to a typed value.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <returns>The typed value.</returns>
        protected virtual T ConvertToTyped<T>(IDictionary<string, object> dictionary)
        {
            if (TypeConverter.TryConvert<IDictionary<string, object>, T>(in dictionary, out T typed))
                return typed;
            else
                throw new InvalidCastException("Failed to convert to the appropriate type.");
        }
        /// <summary>
        /// Converts the specified typed value to a dictionary.
        /// </summary>
        /// <param name="typed">The typed value.</param>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <returns>The dictionary.</returns>
        protected virtual IDictionary<string, object> ConvertFromTyped<T>(T typed)
        {
            if (TypeConverter.TryConvert<T, IDictionary<string, object>>(in typed, out IDictionary<string, object> dictionary))
                return dictionary;
            else
                throw new InvalidCastException("Failed to convert from the appropriate type.");
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
            TInput input = ConvertToTyped<TInput>(job.Input);
            TOutput output = await Perform(input, job);
            foreach (KeyValuePair<string, object> entry in ConvertFromTyped<TOutput>(output))
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
            TInput input = ConvertToTyped<TInput>(job.Input);
            return await IsDeterministic(input);
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
                        Schema = OperationHelper.GenerateSchema(property)
                    }
                );
            }
        }
        /// <inheritdoc />
        public override IAsyncEnumerable<OperationFieldDescriptor> GetInputFields(OperationJob job)
        {
            TInput input = ConvertToTyped<TInput>(job.Input);
            return GetInputFields(input, job);
        }
        /// <summary>
        /// Gets the output fields and their descriptors for this operation.
        /// </summary>
        /// <param name="input">The typed input to the operation.</param>
        /// <param name="job">The executing operation job.</param>
        /// <returns>An enumeration of output field descriptors.</returns>
        public virtual async IAsyncEnumerable<OperationFieldDescriptor> GetOutputFields(TInput input, OperationJob job)
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
        public override IAsyncEnumerable<OperationFieldDescriptor> GetOutputFields(OperationJob job)
        {
            TInput input = ConvertToTyped<TInput>(job.Input);
            return GetOutputFields(input, job);
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
            TInput input = ConvertToTyped<TInput>(job.Input);
            return GetExternalInputFields(input, job);
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
        public override IAsyncEnumerable<OperationFieldDescriptor> GetExternalOutputFields(OperationJob job)
        {
            TInput input = ConvertToTyped<TInput>(job.Input);
            return GetExternalOutputFields(input, job);
        }

        /// <summary>
        /// Gets the template information associated with this operation.
        /// This information contains precisely enough to instantiate an operation without knowing the type.
        /// </summary>
        /// <param name="defaults">The default values for the operation. Optional.</param>
        /// <returns>The operation template.</returns>
        public OperationTemplate GetTemplate(TInput defaults)
        {
            IDictionary<string, object> untypedDefaults = ConvertFromTyped<TInput>(defaults);
            return GetTemplate(untypedDefaults);
        }
    }
}