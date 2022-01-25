using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Operations;
using NUnit.Framework;

namespace CartaTest.Operations
{
    /// <summary>
    /// Tests the performance of the <see cref="OutputOperation" /> operation.
    /// </summary>
    public class TestOutputOperation
    {
        /// <summary>
        /// Test that the output operation acts as expected when a valid context is provided.
        /// </summary>
        /// <param name="expectedKey">The key to expect on the context.</param>
        /// <param name="expectedValue">The value to expect from the context</param>
        /// <param name="key">The key to specify on the output.</param>
        /// <param name="value">The value to specify on the output</param>
        /// <param name="excepts">Whether the operation should be expected to raise an exception.</param>
        /// <param name="equals">Whether the operation should return a value that matches the original value.</param>
        [TestCase("foo", 42, "foo", 42, false, true)]
        [TestCase("bar", 0, "bar", 0, false, true)]
        [TestCase("test", "test", "test", "test", false, true)]
        [TestCase("foo", 42, "bar", 42, true, true)]
        [TestCase("test1", "test", "test2", "test", true, true)]
        [TestCase("foo", 32, "foo", 42, false, false)]
        [TestCase("a", "apple", "a", "cherry", false, false)]
        public async Task TestValidOutputWithContext(
            string expectedKey,
            object expectedValue,
            string key,
            object value,
            bool excepts,
            bool equals)
        {
            // Create a context that the operation can reference when executing.
            OperationContext workflowContext = new()
            {
                Operation = null,
                Input = new Dictionary<string, object>(),
                Output = new Dictionary<string, object>()
            };
            OperationContext operationContext = new()
            {
                Parent = workflowContext,
            };

            // Create the operation and input/output state.
            OutputOperation operation = new();
            OutputOperationIn input = new() { Name = key, Value = value };
            await operation.Perform(input, operationContext);

            if (excepts)
            {
                // We should not expect the correct value to have been assigned.
                Assert.IsFalse(workflowContext.Output.ContainsKey(expectedKey));
            }
            else
            {
                // If an exception was not thrown, check for equality between values output and expected.
                Assert.Contains(expectedKey, workflowContext.Output.Keys);
                object actualValue = workflowContext.Output[expectedKey];
                if (equals)
                    Assert.AreEqual(expectedValue, actualValue);
                else
                    Assert.AreNotEqual(expectedValue, actualValue);
            }
        }
    }
}