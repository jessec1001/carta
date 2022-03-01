using System;
using System.Collections.Generic;
using System.Reflection;
using CartaCore.Documentation;
using CartaCore.Extensions.Documentation;
using CartaCore.Operations.Attributes;

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
        /// If not specified, the executing assembly is used.
        /// </param>
        /// <returns>An enumeration of operation types.</returns>
        public static IEnumerable<Type> FindOperationTypes(Assembly assembly = null)
        {
            // Default to this assembly if no assembly is specified.
            assembly ??= Assembly.GetExecutingAssembly();

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
        /// If not specified, the executing assembly is used.
        /// </param>
        /// <returns>An enumeration of operation types.</returns>
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
            try { description.Description = type.GetDocumentation<StandardDocumentation>().Summary; }
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
        /// <param name="assembly">
        /// The assembly to search for operation types within.
        /// If not specified, the executing assembly is used.
        /// </param>
        /// <returns>An enumeration of operation descriptions.</returns>
        public static IEnumerable<OperationDescription> DescribeOperationTypes(Assembly assembly = null)
        {
            // Get the types of operations.
            IEnumerable<Type> operationTypes = FindOperationTypes(assembly);

            // Generate the descriptions for each of the operation types.
            foreach (Type type in operationTypes)
            {
                // Get the description for this type.
                OperationDescription description = DescribeOperationType(type);

                // If the description is valid, yield it.
                yield return description;
            }
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
        #endregion
    }
}