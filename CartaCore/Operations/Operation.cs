using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CartaCore.Operations.Attributes;
using CartaCore.Typing;

namespace CartaCore.Operations
{
    /// <summary>
    /// Represents an abstract base for an operation that takes an input mapping of named entries and produces a similar
    /// output mapping of named entries.
    /// </summary>
    public abstract class Operation
    {
        /// <summary>
        /// A unique identifier for this operation that should be used for specifying references to this operation.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The default values of the operation.
        /// </summary>
        public Dictionary<string, object> Defaults { get; set; } = new();

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
        /// The default type converter stack used for converting input and output values to the correct type.
        /// </summary>
        public static readonly TypeConverterContext TypeConverter = new(
            new EnumTypeConverter(),
            new NumericTypeConverter(),
            new ArrayTypeConverter(),
            new DictionaryTypeConverter(),
            new AsyncEnumerableTypeConverter(),
            new EnumerableTypeConverter(),
            new SyncAsyncCollectionTypeConverter()
        );

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
        /// Operates on a specified operation job containing input and output mappings. Most operations will use the
        /// input mapping to produce an output mapping. This method is implemented by concrete subclasses.
        /// </summary>
        /// <param name="job">
        /// The job for the calling operation. Will be assigned to a value that provides access to inputs, outputs,
        /// defaults, and configurations for the current operation. When operation inside of a workflow, a parent job is
        /// also provided.
        /// </param>
        /// <returns>
        /// Nothing. The implementation is expected to set values on the input or output of the job.
        /// </returns>
        public abstract Task Perform(OperationJob job);

        /// <summary>
        /// Determines whether the operation is deterministic or non-deterministic on a specified job. This allows
        /// for operations to be memoized. By default, operations are assumed to be deterministic, and thus, memoized.
        /// </summary>
        /// <param name="job">
        /// The job for the calling operation. Will be assigned to a value that provides access to inputs, outputs,
        /// defaults, and configurations for the current operation. When operation inside of a workflow, a parent
        /// job is also provided.
        /// </param>
        /// <returns><c>true</c> if the operation is deterministic on a job; otherwise <c>false</c>.</returns>
        public virtual Task<bool> IsDeterministic(OperationJob job) => Task.FromResult(true);

        /// <summary>
        /// Gets the input fields and their descriptors for this operation.
        /// </summary>
        /// <param name="job">The executing operation job.</param>
        /// <returns>An enumeration of input field descriptors.</returns>
        public abstract IAsyncEnumerable<OperationFieldDescriptor> GetInputFields(OperationJob job);
        /// <summary>
        /// Gets the output fields and their descriptors for this operation.
        /// </summary>
        /// <param name="job">The executing operation job.</param>
        /// <returns>An enumeration of output field descriptors.</returns>
        public abstract IAsyncEnumerable<OperationFieldDescriptor> GetOutputFields(OperationJob job);

        /// <summary>
        /// Gets the input fields of the executing job and their descriptors for this operation.
        /// </summary>
        /// <param name="job">The executing operation job.</param>
        /// <returns>An enumeration of external input field descriptors.</returns>
        public virtual IAsyncEnumerable<OperationFieldDescriptor> GetExternalInputFields(OperationJob job)
        {
            return Enumerable.Empty<OperationFieldDescriptor>().ToAsyncEnumerable();
        }
        /// <summary>
        /// Gets the output fields of the executing job and their descriptors for this operation.
        /// </summary>
        /// <param name="job">The executing operation job.</param>
        /// <returns>An enumeration of external output field descriptors.</returns>
        public virtual IAsyncEnumerable<OperationFieldDescriptor> GetExternalOutputFields(OperationJob job)
        {
            return Enumerable.Empty<OperationFieldDescriptor>().ToAsyncEnumerable();
        }

        /// <summary>
        /// Fetches tasks that need to be executed in order for the operation to complete before starting the operation.
        /// </summary>
        /// <param name="job">The job under which the operation is executing.</param>
        /// <returns>A list of tasks.</returns>
        public virtual IAsyncEnumerable<OperationTask> PrefetchTasks(OperationJob job)
        {
            return Enumerable.Empty<OperationTask>().ToAsyncEnumerable();
        }

        /// <summary>
        /// Gets the template information associated with this operation.
        /// This information contains precisely enough to instantiate an operation without knowing the type.
        /// </summary>
        /// <param name="defaults">The default values for the operation. Optional.</param>
        /// <returns>The operation template.</returns>
        public virtual OperationTemplate GetTemplate(IDictionary<string, object> defaults = null)
        {
            // Get the operation name attribute assosicated with this operation.
            OperationNameAttribute attr = GetType().GetCustomAttribute<OperationNameAttribute>();
            if (attr is null) return null;
            else return new OperationTemplate()
            {
                Type = attr.Type,
                Subtype = null,
                Defaults = defaults
            };
        }
    }
}