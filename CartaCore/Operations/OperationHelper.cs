using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CartaCore.Documentation;
using CartaCore.Extensions.Documentation;
using CartaCore.Operations.Attributes;
using NJsonSchema;
using NJsonSchema.Generation;

namespace CartaCore.Operations
{
    /// <summary>
    /// Helpers for working with operations and details about operations.
    /// </summary>
    public static class OperationHelper
    {
        #region Reflection
        /// <summary>
        /// Finds all of the concretely-implemented, public, operation types within a particular assembly.
        /// </summary>
        /// <param name="assembly">
        /// The assembly to search for operation types within.
        /// If not specified, the operations assembly is used.
        /// </param>
        /// <returns>An enumeration of operation types.</returns>
        public static IEnumerable<Type> FindOperationTypes(Assembly assembly = null)
        {
            // Default to this assembly if no assembly is specified.
            assembly ??= Assembly.GetAssembly(typeof(Operation));

            // Get the types in this assembly that represent operations.
            Type[] assemblyTypes = assembly.GetExportedTypes();
            foreach (Type type in assemblyTypes)
            {
                // If the type is an operation, yield it.
                if (type.IsAssignableTo(typeof(Operation)) &&
                    type.IsPublic && !type.IsAbstract && !type.IsInterface) yield return type;
            }
        }
        /// <summary>
        /// Finds a particular operation type within a particular assembly.
        /// </summary>
        /// <param name="name">
        /// The name of the operation to find.
        /// This is specified via the <see cref="OperationNameAttribute" />.
        /// </param>
        /// <param name="assembly">
        /// The assembly to search for operation types within.
        /// If not specified, the operations assembly is used.
        /// </param>
        /// <returns>The type matching the specified name if found. Otherwise, <c>null</c>.</returns>
        public static Type FindOperationType(string name, Assembly assembly = null)
        {
            // Get the types of operations.
            IEnumerable<Type> operationTypes = FindOperationTypes(assembly);

            // Find the type with the specified name by computing its description.
            foreach (Type type in operationTypes)
            {
                // Create an operation description.
                OperationDescription description = DescribeOperationType(type);
                if (description.Type == name) return type;
            }
            return null;
        }
        /// <summary>
        /// Finds a particular selector operation type within a particular assembly.
        /// </summary>
        /// <param name="selector">
        /// The name of the selector to find.
        /// This is specified via the <see cref="OperationSelectorAttribute" />.
        /// </param>
        /// <param name="assembly">
        /// The assembly to search for operation types within.
        /// If not specified, the operations assembly is used.
        /// </param>
        /// <returns>The type matching the specified selector if found. Otherwise, <c>null</c>.</returns>
        public static Type FindSelectorType(string selector, Assembly assembly = null)
        {
            // Get the types of operations.
            IEnumerable<Type> operationTypes = FindOperationTypes(assembly);

            foreach (Type type in operationTypes)
            {
                OperationSelectorAttribute selectorAttribute = type.GetCustomAttribute<OperationSelectorAttribute>();
                if (selectorAttribute is not null && selectorAttribute.Selector == selector) return type;
            }
            return null;
        }

        /// <summary>
        /// Creates a description from a particular type of operation.
        /// </summary>
        /// <param name="type">The type of operation.</param>
        /// <returns>The generated description.</returns>
        public static OperationDescription DescribeOperationType(Type type)
        {
            // Create an operation description. 
            OperationDescription description = new() { Tags = Array.Empty<string>() };

            // Get the default description from the XML documentation.
            // We ignore errors here because they are more like warnings in this context and will be caught by tests.
            try { description.Description = type.GetDocumentation<StandardDocumentation>().UncontractedSummary; }
            catch { }

            // Apply all description modifying attributes.
            foreach (Attribute attr in type.GetCustomAttributes())
            {
                if (attr is IOperationDescribingAttribute describer)
                    description = describer.Modify(description);
            }

            return description;
        }
        /// <summary>
        /// Creates a description from a particular type of operation.
        /// </summary>
        /// <typeparam name="T">The type of operation.</typeparam>
        /// <returns>The generated description.</returns>
        public static OperationDescription DescribeOperationType<T>() where T : Operation
        {
            return DescribeOperationType(typeof(T));
        }
        /// <summary>
        /// Creates descriptions of all concretely-implemented, public, operation types within a particular assembly.
        /// </summary>
        /// <param name="includeHidden">
        /// Whether to include operations that have been marked with the <see cref="OperationHiddenAttribute" />.
        /// </param>
        /// <param name="assembly">
        /// The assembly to search for operation types within.
        /// If not specified, the operations assembly is used.
        /// </param>
        /// <returns>An enumeration of operation descriptions.</returns>
        public static IEnumerable<OperationDescription> DescribeOperationTypes(bool includeHidden = false, Assembly assembly = null)
        {
            // Get the types of operations.
            IEnumerable<Type> operationTypes = FindOperationTypes(assembly);

            // Generate the descriptions for each of the operation types.
            foreach (Type type in operationTypes)
            {
                if (includeHidden || type.GetCustomAttribute<OperationHiddenAttribute>() is null)
                {

                    // Get the description for this type.
                    OperationDescription description = DescribeOperationType(type);

                    // If the description is valid, yield it.
                    yield return description;
                }
            }
        }
        /// <summary>
        /// Creates a description from a particular instance of an operation.
        /// </summary>
        /// <param name="operation">The instance of operation.</param>
        /// <returns>The generated description.</returns>
        public static OperationDescription DescribeOperationInstance(Operation operation)
        {
            OperationDescription description = DescribeOperationType(operation.GetType());
            if (operation is WorkflowOperation workflow)
                description.Subtype = workflow.SubId;
            return description;
        }
        #endregion

        #region Construction
        // TODO: Add documentation and method for resolving types within a workflow.
        /// <summary>
        /// Constructs a new operation from a specified type.
        /// If the operation type contains any generic type parameters, they are replaced with <see cref="object" />.
        /// </summary>
        /// <param name="type">The type of the operation to construct.</param>
        /// <param name="input">The default input for the operation.</param>
        /// <param name="output">The default output for the operation.</param>
        /// <returns>The newly constructed operation.</returns>
        public static Operation Construct(Type type, out object input, out object output)
        {
            // Check if the type is null.
            if (type is null) throw new ArgumentNullException(nameof(type));

            // We need to check if the type is generic.
            // If it is, we assign all of the generic parameters to `object`.
            if (type.ContainsGenericParameters)
            {
                // Create the generic type with the `object` parameters.
                Type[] typeArguments = new Type[type.GetGenericArguments().Length];
                for (int i = 0; i < typeArguments.Length; i++)
                    typeArguments[i] = typeof(object);
                type = type.MakeGenericType(typeArguments);
            }

            // Create an instance of the operation.
            Operation operation = (Operation)Activator.CreateInstance(type);

            // Construct the input and output values depending on the operation type.
            Type baseType = type.BaseType;
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
                input = output = null;
            }

            return operation;
        }
        /// <summary>
        /// Constructs a new operation from a specified type.
        /// If the operation type contains any generic type parameters, they are replaced with <see cref="object" />.
        /// </summary>
        /// <param name="type">The type of the operation to construct.</param>
        /// <returns>The newly constructed operation.</returns>
        public static Operation Construct(Type type)
        {
            return Construct(type, out _, out _);
        }
        /// <summary>
        /// Constructs a new selector operation from a specified type.
        /// </summary>
        /// <param name="type">The type of selector to construct.</param>
        /// <param name="parameters">The default parameters for the selector.</param>
        /// <returns>The newly constructed selector.</returns>
        public static object ConstructSelector(Type type, out object parameters)
        {
            // Check if the type is null and is assignable to the correct ype.
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            bool typeCheckPassed = false;
            Type[] implInterfaces = type.GetInterfaces();
            foreach (Type implInterface in implInterfaces)
            {
                if (implInterface.IsGenericType && implInterface.GetGenericTypeDefinition() == typeof(ISelector<,>))
                {
                    typeCheckPassed = true;
                    break;
                }
            }
            if (!typeCheckPassed)
                throw new ArgumentException("The specified type is not a selector.", nameof(type));

            // Create an instance of the selector and parameters.
            object selector = Activator.CreateInstance(type);
            Type selectorParameterType = (Type)type
                .GetProperty(nameof(ISelector<object, object>.ParameterType))
                .GetValue(selector);
            parameters = Activator.CreateInstance(selectorParameterType);
            return selector;
        }
        /// <summary>
        /// Constructs a new selector operation from a specified type.
        /// </summary>
        /// <param name="type">The type of selector to construct.</param>
        /// <param name="parameters">The default parameters for the selector.</param>
        /// <typeparam name="TSource">The source type of the selector.</typeparam>
        /// <typeparam name="TTarget">The target type of the selector.</typeparam>
        /// <returns>The newly constructed selector.</returns>
        public static ISelector<TSource, TTarget> ConstructSelector<TSource, TTarget>(Type type, out object parameters)
        {
            // Check if the type is null and is assignable to the correct type.
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (!type.IsAssignableTo(typeof(ISelector<TSource, TTarget>)))
                throw new ArgumentException("The specified type is not a selector.", nameof(type));

            // Create an instance of the selector and parameters.
            ISelector<TSource, TTarget> selector = (ISelector<TSource, TTarget>)Activator.CreateInstance(type);
            parameters = Activator.CreateInstance(selector.ParameterType);
            return selector;
        }
        #endregion

        #region Schema
        /// <summary>
        /// Generates a JSON schema for a particular type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The generated JSON schema.</returns>
        public static JsonSchema GenerateSchema(Type type)
        {
            // We setup the schema generator.
            JsonSchema schema;
            JsonSchemaGeneratorSettings schemaGeneratorSettings = new()
            {
                GenerateAbstractSchemas = false,
                FlattenInheritanceHierarchy = true,
                GenerateEnumMappingDescription = true,
            };

            // The default treatment of streams as nullable objects needs to be overridden.
            // We assume that all streams are files.
            if (type.IsAssignableTo(typeof(Stream)))
                schema = new JsonSchema { Type = JsonObjectType.File };
            else
                schema = JsonSchema.FromType(type, schemaGeneratorSettings);

            // The schema generation does not handle nullable types by default.
            // We need to manually add the nullable type to the schema.
            if (type.IsClass || type.IsInterface || Nullable.GetUnderlyingType(type) is not null)
                schema.Type |= JsonObjectType.Null;

            // We make sure that no title was specified.
            schema.Title = null;

            return schema;
        }
        /// <summary>
        /// Generates a JSON schema for a particular type with some attributes to apply.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="attributes">The attributes.</param>
        /// <returns>The generated JSON schema.</returns>
        public static JsonSchema GenerateSchema(Type type, IEnumerable<Attribute> attributes)
        {
            // Generate the base schema.
            JsonSchema schema = GenerateSchema(type);

            // Apply any schema-modifying attributes to the schema.
            foreach (Attribute attribute in attributes)
            {
                // Check if the attribute is a schema-modifying attribute and apply if so.
                if (attribute is ISchemaModifierAttribute schemaAttribute)
                    schema = schemaAttribute.ModifySchema(schema);
            }

            return schema;
        }
        /// <summary>
        /// Generates a JSON schema for a property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The generated JSON schema.</returns>
        public static JsonSchema GenerateSchema(PropertyInfo property)
        {
            // Generate the base schema.
            JsonSchema schema = GenerateSchema(property.PropertyType, property.GetCustomAttributes());

            // Try to get documentation for the property.
            StandardDocumentation documentation;
            try { documentation = property.GetDocumentation<StandardDocumentation>(); }
            catch { documentation = null; }

            // Apply the property name and description.
            schema.Title ??= property.Name;
            schema.Description ??= documentation?.UncontractedSummary;

            return schema;
        }
        #endregion
    }
}