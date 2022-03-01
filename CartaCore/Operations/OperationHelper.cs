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
        /// Create a description from a particular type of operation.
        /// </summary>
        /// <param name="type">The type of operation.</param>
        /// <returns>The generated description.</returns>
        public static OperationDescription DescribeOperationType(Type type)
        {
            // Create an operation description. 
            OperationDescription description = new() { Tags = Array.Empty<string>() };

            // Get the default description from the XML documentation.
            // We ignore errors here because they are more like warnings in this context and will be caught by tests.
            try
            {
                description.Description = type.GetDocumentation<StandardDocumentation>().Summary;
            }
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
        /// Create a description from a particular type of operation.
        /// </summary>
        /// <typeparam name="T">The type of operation.</typeparam>
        /// <returns>The generated description.</returns>
        public static OperationDescription DescribeOperationType<T>() where T : Operation
        {
            return DescribeOperationType(typeof(T));
        }
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
        public static Operation Construct(
            Type type,
            out object input,
            out object output,
            WorkflowOperation workflow = null)
        {

        }
        // TODO: Implement constructing generic operations if necessary.
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
        #endregion
    }
}