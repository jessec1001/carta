using System;
using System.Linq;
using System.Reflection;
using CartaCore.Documentation;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    /// <summary>
    /// A description of an operation including its unique type identity, naming, description, and tagging information.
    /// </summary>
    public struct OperationDescription
    {
        /// <summary>
        /// The type name of an operation. This will be unique across all operations and can be used as an
        /// identifier/discriminant for a particular operation code-type. 
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// The subtype name of an operation. This applies to operations of type "workflow" and similar. This will be
        /// unique per templated operation of a specific <see cref="Type" />.
        /// </summary>
        public string Subtype { get; set; }
        /// <summary>
        /// The name of this operation when used as a selector for a graph. If the operation is not a selector, this
        /// value will be null.
        /// </summary>
        public string Selector { get; set; }
        /// <summary>
        /// The display name of an operation. This need not be unique across all operations. However, it should provide
        /// a self-explanatory name to an operation that allows a user to intuitively guess the functionality of an
        /// operation.
        /// </summary>
        public string Display { get; set; }

        /// <summary>
        /// A more detailed description of an operation functionality. This should be where in-depth details are given
        /// to allow the user to understand more about how an operation is performed. This field should be assumed to
        /// support Markdown syntax when interpreted by a client application.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A collection of descriptive categories that apply to an operation. These should be designed to provide
        /// additional context clues to the nature of an operation. 
        /// </summary>
        public string[] Tags { get; set; }

        /// <summary>
        /// Create a description from a particular type of operation.
        /// </summary>
        /// <param name="type">The type of operation.</param>
        /// <returns>The generated description.</returns>
        public static OperationDescription FromType(Type type)
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
            foreach (OperationDescribingAttribute attr in type.GetCustomAttributes<OperationDescribingAttribute>())
                description = attr.Modify(description);

            return description;
        }
        /// <summary>
        /// Create a description from a particular type of operation.
        /// </summary>
        /// <typeparam name="T">The type of operation.</typeparam>
        /// <returns>The generated description.</returns>
        public static OperationDescription FromType<T>() where T : Operation
        {
            return FromType(typeof(T));
        }

        /// <summary>
        /// Creates the descriptions for all concretely-implemented public operations.
        /// </summary>
        /// <returns>A list operation descriptions for all operation types.</returns>
        public static OperationDescription[] FromOperationTypes(Assembly assembly = null)
        {
            // Use the specified or default assembly.
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
                descriptions[k] = FromType(operationTypes[k]);
            return descriptions;
        }
    }
}