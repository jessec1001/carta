using System.Collections.Generic;
using CartaCore.Operations;
using NUnit.Framework;

namespace CartaTest.Operations
{
    /// <summary>
    /// Tests the performance of the <see cref="InputOperation" /> operation.
    /// </summary>
    public class TestInputOperation
    {
        /// <summary>
        /// Tests that the input operation acts as expected when a valid context is provided.
        /// </summary>
        /// <param name="expectedKey">The key to expect on the input.</param>
        /// <param name="expectedValue">The value to expect from the input.</param>
        /// <param name="key">The key to specify on the context.</param>
        /// <param name="value">The value to specify on the context.</param>
        /// <param name="excepts">Whether the operation should be expected to raise an exception.</param>
        /// <param name="equals">Whether the operation should return a value that matches the original value.</param>
        [TestCase("foo", 42, "foo", 42, false, true)]
        [TestCase("bar", 0, "bar", 0, false, true)]
        [TestCase("test", "test", "test", "test", false, true)]
        [TestCase("foo", 42, "bar", 42, true, true)]
        [TestCase("test1", "test", "test2", "test", true, true)]
        [TestCase("foo", 32, "foo", 42, false, false)]
        [TestCase("a", "apple", "a", "cherry", false, false)]
        public void TestValidInputWithContext(
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
                Input = new Dictionary<string, object>()
                {
                    [key] = value
                },
                Output = new Dictionary<string, object>()
            };

            // Create the operation and input/output state.
            InputOperation operation = new();
            InputOperationIn input = new() { Name = expectedKey };
            InputOperationOut output;

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

                // If an exception was not thrown, check for equality.
                object actualValue = output.Value;
                if (equals)
                    Assert.AreEqual(expectedValue, actualValue);
                else
                    Assert.AreNotEqual(expectedValue, actualValue);
            }
        }

        /// <summary>
        /// Tests that the input operation does not work when a context is not provided.
        /// </summary>
        [Test]
        public void TestInputWithoutContext()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                InputOperation operation = new();
                InputOperationIn input = new() { Name = "foo" };
                InputOperationOut output = await operation.Perform(input);
            });
        }
    }
}