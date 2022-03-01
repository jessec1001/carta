using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CartaCore.Extensions.Typing;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    // TODO: Implement something, perhaps in the task running service, that will automatically handle loading uploaded
    //       files into streams and handle saving streams to downloadable files.

    /// <summary>
    /// Represents an abstract base for an operation that takes an input mapping of named entries and produces a similar
    /// output mapping of named entries.
    /// </summary>
    public abstract class Operation
    {
        /// <summary>
        /// A unique identifier for this operation that should be used for specifying references to this operation.
        /// </summary>
        public string Id { get; init; }
        /// <summary>
        /// The default values of the operation.
        /// </summary>
        public Dictionary<string, object> Defaults { get; set; } = new();

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
        public virtual bool IsDeterministic(OperationJob job) => true;

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
        /// Get the tasks that need to be executed in order for the operation to complete.
        /// </summary>
        /// <param name="job">The job under which the operation is executing.</param>
        /// <returns>A list of tasks.</returns>
        public virtual IAsyncEnumerable<OperationTask> GetTasks(OperationJob job)
        {
            return Enumerable.Empty<OperationTask>().ToAsyncEnumerable();
        }

        // // TODO: Reimplement elsewhere.
        // /// <summary>
        // /// Generates a schema for an input field specified by its name.
        // /// </summary>
        // /// <param name="field">The name of the field.</param>
        // /// <returns>A generated JSON schema.</returns>
        // public virtual JsonSchema GetInputFieldSchema(string field)
        // {
        //     Type fieldType = GetInputFieldType(field);

        //     JsonSchema jsonSchema;
        //     JsonSchemaGeneratorSettings jsonSettings = new JsonSchemaGeneratorSettings()
        //     {
        //         GenerateAbstractSchemas = false,
        //         FlattenInheritanceHierarchy = true
        //     };

        //     if (fieldType.IsAssignableTo(typeof(Stream)))
        //         jsonSchema = new JsonSchema { Type = JsonObjectType.File };
        //     else
        //         jsonSchema = JsonSchema.FromType(fieldType, jsonSettings);

        //     // TODO: Figure out the proper way to handle this schema generation.
        //     // For now, we need to make reference types nullable manually at the root level.
        //     Type nullableType = Nullable.GetUnderlyingType(fieldType);
        //     if (fieldType.IsClass || nullableType != null)
        //         jsonSchema.Type |= JsonObjectType.Null;

        //     jsonSchema.Title = field;
        //     return jsonSchema;
        // }
        // /// <summary>
        // /// Generates a schema for an output field specified by its name.
        // /// </summary>
        // /// <param name="field">The name of the field.</param>
        // /// <returns>A generated JSON schema.</returns>
        // public virtual JsonSchema GetOutputFieldSchema(string field)
        // {
        //     Type fieldType = GetOutputFieldType(field);

        //     JsonSchema jsonSchema;
        //     JsonSchemaGeneratorSettings jsonSettings = new JsonSchemaGeneratorSettings()
        //     {
        //         GenerateAbstractSchemas = false,
        //         FlattenInheritanceHierarchy = true
        //     };

        //     if (fieldType.IsAssignableTo(typeof(Stream)))
        //         jsonSchema = new JsonSchema { Type = JsonObjectType.File };
        //     else
        //         jsonSchema = JsonSchema.FromType(fieldType, jsonSettings);

        //     // TODO: Figure out the proper way to handle this schema generation.
        //     // For now, we need to make reference types nullable manually at the root level.
        //     // For now, we need to make nullable value types nullable manually at the root level.
        //     Type nullableType = Nullable.GetUnderlyingType(fieldType);
        //     if (fieldType.IsClass || nullableType != null)
        //         jsonSchema.Type |= JsonObjectType.Null;

        //     jsonSchema.Title = field;
        //     return jsonSchema;
        // }
        

        // TODO: Move into helpers.
        /// <summary>
        /// Constructs a new selector operation from a specified selector type.
        /// </summary>
        /// <param name="selector">The selector type name of the operation to construct.</param>
        /// <param name="input">The default input for the operation.</param>
        /// <param name="output">The default output for the operation.</param>
        /// <param name="assembly">The assembly to find the operation inside of.</param>
        /// <returns>The newly constructed operation.</returns>
        public static Operation ConstructSelector(string selector, out object input, out object output, Assembly assembly = null)
        {
            // TODO: This contains mostly duplicated code from `OperationDescription`.
            // Assign the assembly to the current assembly if it is not specified.
            assembly ??= Assembly.GetAssembly(typeof(Operation));

            // Get the concrete types which are implementations of an operation.
            Type[] assemblyTypes = assembly.GetTypes();
            Type[] operationTypes = assemblyTypes
                .Where(type =>
                    type.IsAssignableTo(typeof(Operation)) &&
                    type.IsPublic &&
                    !(type.IsAbstract || type.IsInterface))
                .ToArray();

            // Generate the descriptions for each of the operation implementation types.
            OperationDescription[] descriptions = new OperationDescription[operationTypes.Length];
            for (int k = 0; k < operationTypes.Length; k++)
            {
                descriptions[k] = OperationDescription.FromType(operationTypes[k]);

                // Find the operation type corresponding to the specified selector.
                if (descriptions[k].Selector == selector)
                {
                    // Construct the operation.
                    Operation operation = (Operation)Activator.CreateInstance(operationTypes[k]);

                    // Construct the input and output values if the operation is a typed operation.
                    Type baseType = operation.GetType().BaseType;
                    if (baseType.IsAssignableTo(typeof(TypedOperation<,>)))
                    {
                        // Get the input and output types.
                        Type inputType = baseType.GetGenericArguments()[0];
                        Type outputType = baseType.GetGenericArguments()[1];

                        // Construct the input and output values.
                        input = Activator.CreateInstance(inputType);
                        output = Activator.CreateInstance(outputType);
                    }
                    else
                    {
                        input = null;
                        output = null;
                    }
                }
            }

            // If the operation type was not found, return null.
            input = output = null;
            return null;
        }
        // TODO: Move into helpers.
        /// <summary>
        /// Executes a selector operation by setting and getting the preset selector graph on the operation.
        /// </summary>
        /// <param name="operation">The selector operation.</param>
        /// <param name="input">The input to the operation.</param>
        /// <param name="graph">The graph to perform the selector on.</param>
        /// <returns>The resulting graph after the selector has been applied.</returns>
        public static async Task<Graph> ExecuteSelector(Operation operation, object input, Graph graph)
        {
            // Find the field on the input object that corresponds to the selector graph.
            foreach (PropertyInfo property in input.GetType().GetProperties())
            {
                // Set the graph value on the input object.
                OperationSelectorGraphAttribute attr = property.GetCustomAttribute<OperationSelectorGraphAttribute>();
                if (attr is not null)
                    property.SetValue(input, graph);
            }

            // Get the input and output types of the operation.
            Type baseType = operation.GetType().BaseType;
            Type inputType = null;
            Type outputType = null;
            if (baseType.IsAssignableTo(typeof(TypedOperation<,>)))
            {
                // Get the input and output types.
                inputType = baseType.GetGenericArguments()[0];
                outputType = baseType.GetGenericArguments()[1];
            }

            // TODO: We need to inherit execution settings from the calling API.
            // Execute the operation.
            OperationJob job = new() { Operation = operation };
            job.Input = input.AsDictionary(inputType, DefaultTypeConverter);
            await operation.Perform(job);
            object output = job.Output.AsTyped(outputType, DefaultTypeConverter);

            // Find the field on the output object that corresponds to the selector graph.
            foreach (PropertyInfo property in output.GetType().GetProperties())
            {
                // Get the graph value from the output object.
                OperationSelectorGraphAttribute attr = property.GetCustomAttribute<OperationSelectorGraphAttribute>();
                if (attr is not null)
                {
                    graph = (Graph)property.GetValue(output);
                    return graph;
                }
            }

            // If the graph was not found, return null.
            return null;
        }
    }
}