using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CartaCore.Operations;
using NUnit.Framework;

namespace CartaTests.Operations
{
    /// <summary>
    /// Tests that all of the operations in the Carta package are correctly labeled with metadata so they may be used
    /// by an API appropriately.
    /// </summary>
    public class TestOperationDescriptions
    {
        /// <summary>
        /// The types of operations that exist within the Carta assembly.
        /// </summary>
        protected Type[] OperationTypes = null;

        /// <summary>
        /// Sets up the tests by finding all of the types of operations.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            // Get the concrete types which are implementations of an operation.
            Type[] assemblyTypes = Assembly.GetAssembly(typeof(Operation)).GetTypes();
            Type[] operationTypes = assemblyTypes
                .Where(type => 
                    type.IsAssignableTo(typeof(Operation)) &&
                    type.IsPublic &&
                    !(type.IsAbstract || type.IsInterface))
                .ToArray();
            OperationTypes = operationTypes;
        }

        /// <summary>
        /// Tests that the type name, display name, and tags for each operation is non-null.
        /// Will raise an assertion error with the class name of the offending operation. 
        /// </summary>
        [Test]
        public void TestNonNull()
        {
            foreach (Type operationType in OperationTypes)
            {
                OperationDescription description = OperationDescription.FromType(operationType);
                Assert.IsNotNull(
                    description.Type,
                    $"'{operationType.Name}' has a null type name. Set it using an `[OperationName]` attribute."
                );
                Assert.IsNotNull(
                    description.Display,
                    $"'{operationType.Name}' has a null display name. Set it using an `[OperationName]` attribute."
                );
                Assert.IsNotNull(
                    description.Description,
                    $"'{operationType.Name}' has a null description. Add XML documentation to its class to add it."
                );
                Assert.IsNotNull(description.Tags);
            }
        }

        /// <summary>
        /// Tests that the type names of all of the operations are unique.
        /// </summary>
        [Test]
        public void TestUniqueTypes()
        {
            // Keep track of known type names and check for collisions.
            Dictionary<string, Type> typeNames = new();
            foreach (Type operationType in OperationTypes)
            {
                OperationDescription description = OperationDescription.FromType(operationType);
                if (typeNames.TryGetValue(description.Type, out Type collisionType))
                    Assert.Fail(
                        $"Operations '{operationType.Name}' and '{collisionType.Name}' have identical type names." +
                        " These must be unique."
                    );
                else
                    typeNames.Add(description.Type, operationType);
            }
        }
    }
}