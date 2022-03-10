using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CartaCore.Operations;
using NUnit.Framework;

namespace CartaTest.Operations
{
    /// <summary>
    /// Tests the performance of the <see cref="OutputOperation{T}" /> operation.
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
            // Create the operation and input/output state.
            OutputOperation<object> operation = new();

            // Create a context that the operation can reference when executing.
            OperationJob workflowJob = new(
                null,
                "workJob",
                new Dictionary<string, object>(),
                new Dictionary<string, object>()
            );
            OperationJob operationJob = new(operation, "opJob", workflowJob);

            // Run the operation.
            OutputOperationIn<object> input = new() { Name = key, Value = value };
            await operation.Perform(input, operationJob);

            if (excepts)
            {
                // We should not expect the correct value to have been assigned.
                Assert.IsFalse(workflowJob.Output.ContainsKey(expectedKey));
            }
            else
            {
                // If an exception was not thrown, check for equality between values output and expected.
                ICollection<string> keys = workflowJob.Output.Keys;
                Assert.Contains(expectedKey, keys.ToArray());
                object actualValue = workflowJob.Output[expectedKey];
                if (equals)
                    Assert.AreEqual(expectedValue, actualValue);
                else
                    Assert.AreNotEqual(expectedValue, actualValue);
            }
        }
    }
}