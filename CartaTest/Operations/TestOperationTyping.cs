using System;
using System.Linq;
using System.Reflection;
using CartaCore.Operations;
using NUnit.Framework;

namespace CartaTests.Operations
{
    /// <summary>
    /// Tests the typing assumptions of operation types related to construction and execution.
    /// </summary>
    public class TestOperationTyping
    {
        /// <summary>
        /// The types of operations that inherit from `Operation` and exist within the Carta assembly.
        /// </summary>
        protected Type[] OperationTypes = null;
        /// <summary>
        /// The types of operations that inherit from `TypedOperation` and exist within the Carta assembly.
        /// </summary>
        protected Type[] TypedOperationTypes = null;

        /// <summary>
        /// Sets up the tests by finding all of the types of operations.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            Type[] assemblyTypes = Assembly.GetAssembly(typeof(Operation)).GetTypes();
            Type[] operationTypes = assemblyTypes
                .Where(type =>
                    type.IsAssignableTo(typeof(Operation)) &&
                    type.IsPublic &&
                    !(type.IsAbstract || type.IsInterface))
                .ToArray();
            Type[] typedOperationTypes = operationTypes
                .Where(type =>
                    type.BaseType is not null &&
                    type.BaseType.IsGenericType &&
                    type.BaseType.GetGenericTypeDefinition().IsAssignableTo(typeof(TypedOperation<,>))
                )
                .ToArray();
            OperationTypes = operationTypes;
            TypedOperationTypes = typedOperationTypes;
        }

        /// <summary>
        /// Tests that all of the members of the input and output types of each operation are properties.
        /// </summary>
        [Test]
        public void TestInputOutputPropertiesOnly()
        {
            foreach (Type operationType in TypedOperationTypes)
            {
                // Get the input and output types of the operation.
                Type[] ioTypes = operationType.BaseType.GetGenericArguments();
                Type inputType = ioTypes[0];
                Type outputType = ioTypes[1];

                // Grab all of the members.
                MemberInfo[] inputProperties = inputType.GetMembers();
                MemberInfo[] outputProperties = outputType.GetMembers();

                // Verify that any public members that are not members are properties.
                foreach (MemberInfo member in inputProperties)
                {
                    if (member is not FieldInfo field) continue;
                    if (field.IsPublic)
                    {
                        Assert.Fail
                        (
                            $"{operationType.Name}'s input type has a public field named '{field.Name}' which should be a property. " +
                            "If you want to expose a field, use a property instead and restrict access to the field."
                        );
                    }
                }
                foreach (MemberInfo member in outputProperties)
                {
                    if (member is not FieldInfo field) continue;
                    if (field.IsPublic)
                    {
                        Assert.Fail
                        (
                            $"{operationType.Name}'s output type has a public field named '{field.Name}' which should be a property. " +
                            "If you want to expose a field, use a property instead and restrict access to the field."
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Tests that all operations have a parameterless constructor.
        /// </summary>
        [Test]
        public void TestParameterlessConstructors()
        {
            // Verify for each operation.
            foreach (Type operationType in OperationTypes)
            {
                // Check that at least one parameterless constructor exists.
                bool parameterlessExists = false;
                ConstructorInfo[] constructors = operationType.GetConstructors();
                foreach (ConstructorInfo constructor in constructors)
                {
                    if (constructor.GetParameters().Length == 0)
                    {
                        parameterlessExists = true;
                        break;
                    }
                }

                // If no parameterless constructor exists, fail.
                if (!parameterlessExists)
                {
                    Assert.Fail
                    (
                        $"{operationType.Name} does not have a parameterless constructor. " +
                        "All operations must have a parameterless constructor."
                    );
                }
            }
        }
    }
}