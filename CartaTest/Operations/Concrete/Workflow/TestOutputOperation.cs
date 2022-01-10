using System.Collections.Generic;
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
        public void TestValidOutputWithContext(
            string expectedKey,
            object expectedValue,
            string key,
            object value,
            bool excepts,
            bool equals)
        {
            // Create a context that the operation can reference when executing.
            OperationContext context = new()
            {
                Operation = null,
                Input = new Dictionary<string, object>(),
                Output = new Dictionary<string, object>()
            };

            // Create the operation and input/output state.
            OutputOperation operation = new();
            OutputOperationIn input = new() { Name = key, Value = value };
            OutputOperationOut output;

            // If we expect an error to be thrown, perform the appropriate assertion.
            if (excepts)
            {
                Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                    output = await operation.Perform(input, context)
                );
            }
            // Otherwise, expect no error to occur in the assertion.
            else
            {
                Assert.DoesNotThrowAsync(async () =>
                    output = await operation.Perform(input, context)
                );

                // If an exception was not thrown, check for equality between values output and expected.
                Assert.Contains(expectedKey, context.Output.Keys);
                object actualValue = context.Output[expectedKey];
                if (equals)
                    Assert.AreEqual(expectedValue, actualValue);
                else
                    Assert.AreNotEqual(expectedValue, actualValue);
            }
        }

        /// <summary>
        /// Tests that the output operation does not work when a context is not provided.
        /// </summary>
        [Test]
        public void TestOutputWithoutContext()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                OutputOperation operation = new();
                OutputOperationIn input = new() { Name = "foo" };
                OutputOperationOut output = await operation.Perform(input);
            });
        }
    }
}