using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CartaCore.Data;
using CartaCore.Extensions.Typing;
using CartaCore.Operations.Attributes;
using CartaCore.Typing.Conversion;
using NJsonSchema;
using NJsonSchema.Generation;

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
        // TODO: Can we make this readonly?
        /// <summary>
        /// A unique identifier for this operation that should be used for specifying references to this operation.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The default values of the operation.
        /// </summary>
        public Dictionary<string, object> DefaultValues { get; set; } = new();

        /// <summary>
        /// The initial values of the operation.
        /// Concrete operations should override this property to provide the initial values of the operation.
        /// </summary>
        public virtual Dictionary<string, object> InitialValues { get => new(); }

        /// <summary>
        /// The default type converter.
        /// </summary>
        protected static readonly TypeConverterContext DefaultTypeConverter = new(
            new EnumTypeConverter(),
            new NumericTypeConverter(),
            new ArrayTypeConverter(),
            new DictionaryTypeConverter()
        );

        // TODO: This needs to be done better to support passing the holistic information about typing, naming, and
        //       schema back to calling code.

        /// <summary>
        /// An enumeration of inputs that this operation requires within the parent context.
        /// </summary>
        public virtual IEnumerable<string> ExternalInputs => Enumerable.Empty<string>();
        /// <summary>
        /// An enumeration of outputs that this operation produces within the parent context.
        /// </summary>
        public virtual IEnumerable<string> ExternalOutputs => Enumerable.Empty<string>();

        /// <summary>
        /// Operates on a specified operation context containing input and output mappings. Most operations will use the
        /// input mapping to produce an output mapping. This method is implemented by concrete subclasses.
        /// </summary>
        /// <param name="context">
        /// The context for the calling operation. Will be assigned to a value that provides access to inputs, outputs,
        /// defaults, and configurations for the current operation. When operation inside of a workflow, a parent
        /// context is also provided.
        /// </param>
        /// <returns>
        /// Nothing. The implementation is expected to set values on the input or output of the context.
        /// </returns>
        public abstract Task Perform(OperationContext context);

        /// <summary>
        /// Determines whether the operation is deterministic or non-deterministic on a specified context. This allows
        /// for operations to be memoized. By default, operations are assumed to be deterministic, and thus, memoized.
        /// </summary>
        /// <param name="context">
        /// The context for the calling operation. Will be assigned to a value that provides access to inputs, outputs,
        /// defaults, and configurations for the current operation. When operation inside of a workflow, a parent
        /// context is also provided.
        /// </param>
        /// <returns><c>true</c> if the operation is deterministic on a context; otherwise <c>false</c>.</returns>
        public virtual bool IsDeterministic(OperationContext context) => true;

        // TODO: Incorporate context into these methods allowing for dynamic input fields.
        /// <summary>
        /// Gets the input fields that are available for this operation.
        /// </summary>
        /// <returns>A list of valid input fields.</returns>
        public abstract Task<string[]> GetInputFields(OperationContext context);
        /// <summary>
        /// Gets the output fields that are available for this operation.
        /// </summary>
        /// <returns>A list of valid output fields.</returns>
        public abstract Task<string[]> GetOutputFields(OperationContext context);

        /// <summary>
        /// Gets the type of an input field specified by its name.
        /// </summary>
        /// <param name="field">The name of the field.</param>
        /// <returns>The type of the input field.</returns>
        public abstract Type GetInputFieldType(string field);
        /// <summary>
        /// Gets the type of an output field specified by its name.
        /// </summary>
        /// <param name="field">The name of the field.</param>
        /// <returns>The type of the output field.</returns>
        public abstract Type GetOutputFieldType(string field);

        /// <summary>
        /// Generates a schema for an input field specified by its name.
        /// </summary>
        /// <param name="field">The name of the field.</param>
        /// <returns>A generated JSON schema.</returns>
        public virtual JsonSchema GetInputFieldSchema(string field)
        {
            Type fieldType = GetInputFieldType(field);

            JsonSchema jsonSchema;
            JsonSchemaGeneratorSettings jsonSettings = new JsonSchemaGeneratorSettings()
            {
                GenerateAbstractSchemas = false,
                FlattenInheritanceHierarchy = true
            };

            if (fieldType.IsAssignableTo(typeof(Stream)))
                jsonSchema = new JsonSchema { Type = JsonObjectType.File };
            else
                jsonSchema = JsonSchema.FromType(fieldType, jsonSettings);

            // TODO: Figure out the proper way to handle this schema generation.
            // For now, we need to make reference types nullable manually at the root level.
            Type nullableType = Nullable.GetUnderlyingType(fieldType);
            if (fieldType.IsClass || nullableType != null)
                jsonSchema.Type |= JsonObjectType.Null;

            jsonSchema.Title = field;
            return jsonSchema;
        }
        /// <summary>
        /// Generates a schema for an output field specified by its name.
        /// </summary>
        /// <param name="field">The name of the field.</param>
        /// <returns>A generated JSON schema.</returns>
        public virtual JsonSchema GetOutputFieldSchema(string field)
        {
            Type fieldType = GetOutputFieldType(field);

            JsonSchema jsonSchema;
            JsonSchemaGeneratorSettings jsonSettings = new JsonSchemaGeneratorSettings()
            {
                GenerateAbstractSchemas = false,
                FlattenInheritanceHierarchy = true
            };

            if (fieldType.IsAssignableTo(typeof(Stream)))
                jsonSchema = new JsonSchema { Type = JsonObjectType.File };
            else
                jsonSchema = JsonSchema.FromType(fieldType, jsonSettings);

            // TODO: Figure out the proper way to handle this schema generation.
            // For now, we need to make reference types nullable manually at the root level.
            // For now, we need to make nullable value types nullable manually at the root level.
            Type nullableType = Nullable.GetUnderlyingType(fieldType);
            if (fieldType.IsClass || nullableType != null)
                jsonSchema.Type |= JsonObjectType.Null;

            jsonSchema.Title = field;
            return jsonSchema;
        }

        // TODO: Provide a better way to retrieve tasks for an operation before executing the operation itself.
        /// <summary>
        /// Get the tasks that need to be executed in order for the operation to complete.
        /// </summary>
        /// <param name="context">The context under which the operation is executing.</param>
        /// <returns>A list of tasks.</returns>
        public virtual async IAsyncEnumerable<OperationTask> GetTasks(OperationContext context)
        {
            yield break;

            // // TODO: Support tasks being added by individual operations at any stage in the workflow execution. For now,
            // //       we will only consider tasks induced by missing inputs to the entire operation.
            // Dictionary<string, object> total = context.Total;
            // foreach (KeyValuePair<string, object> entry in total)
            // {
            //     // This check for tasks should really only be available for `InputOperation`.
            //     if (entry.Value is Stream stream && stream == Stream.Null)
            //     {
            //         yield return new OperationTask()
            //         {
            //             Type = OperationTaskType.File,
            //             Field = entry.Key
            //         };
            //     }
            // }
        }

        /// <summary>
        /// Gets an operation type by its type name if it exists.
        /// </summary>
        /// <returns>
        /// The type which is assignable to type <see cref="Operation" />. May be null if such a type does not exist.
        /// </returns>
        public static Type TypeFromName(string name)
        {
            Type[] assemblyTypes = Assembly.GetAssembly(typeof(Operation)).GetTypes();
            Type operationType = assemblyTypes
                .FirstOrDefault(type =>
                    type.IsAssignableTo(typeof(Operation)) &&
                    type.IsPublic &&
                    !(type.IsAbstract || type.IsInterface) &&
                    type.GetCustomAttribute<OperationNameAttribute>()?.Type == name
                );
            return operationType;
        }

        /// <summary>
        /// Constructs a new operation from a specified type.
        /// </summary>
        /// <param name="type">The type name of the operation to construct.</param>
        /// <param name="input">The default input for the operation.</param>
        /// <param name="output">The default output for the operation.</param>
        /// <param name="assembly">The assembly to find the operation inside of.</param>
        /// <returns>The newly constructed operation.</returns>
        public static Operation Construct(string type, out object input, out object output, Assembly assembly = null)
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

                // Find the operation type corresponding to the specified type.
                if (descriptions[k].Type == type)
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
        /// <summary>
        /// Contructs a new operation from a specified type.
        /// </summary>
        /// <param name="type">The type name of the operation to construct.</param>
        /// <param name="assembly">The assembly to find the operation inside of.</param>
        /// <returns>The newly constructed operation.</returns>
        public static Operation Construct(string type, Assembly assembly = null)
        {
            return Construct(type, out _, out _, assembly);
        }
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
            OperationContext context = new() { Operation = operation };
            context.Input = input.AsDictionary(inputType, DefaultTypeConverter);
            await operation.Perform(context);
            object output = context.Output.AsTyped(outputType, DefaultTypeConverter);

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