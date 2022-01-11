using System;
using System.Threading.Tasks;
using CartaCore.Operations;
using NUnit.Framework;

namespace CartaTest.Operations
{
    /// <summary>
    /// Tests the performance of the <see cref="ConvertToNumberOperation" /> operation.
    /// </summary>
    public class TestConvertToNumberOperation
    {
        /// <summary>
        /// Tests that the <see cref="ConvertToNumberOperation" /> operation converts valid number formats correctly.
        /// </summary>
        /// <param name="expectedValue">The expected converted number value.</param>
        /// <param name="text">The input string value.</param>
        [TestCase(1, "1")]
        [TestCase(1, "1e0")]
        [TestCase(3.14, "3.14")]
        [TestCase(0.14, ".14")]
        [TestCase(-2.728, "-2.728")]
        [TestCase(1234, "1.234e3")]
        [TestCase(-98765, "-9.8765e4")]
        public async Task TestConvertValid(double expectedValue, string text)
        {
            ConvertToNumberOperation operation = new();
            ConvertToNumberOperationIn input = new() { Text = text };
            ConvertToNumberOperationOut output = await operation.Perform(input);

            Assert.AreEqual(expectedValue, output.Number, 1e-4);
        }
        /// <summary>
        /// Tests that the <see cref="ConvertToNumberOperation" /> operation throws an exception when the input is not a
        /// valid number.
        /// </summary>
        /// <param name="text">The input string value.</param>
        [TestCase("--3")]
        [TestCase("abc")]
        [TestCase("1.2.3")]
        [TestCase("4-")]
        [TestCase(null)]
        public void TestConvertInvalid(string text)
        {
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                ConvertToNumberOperation operation = new();
                ConvertToNumberOperationIn input = new() { Text = text };
                ConvertToNumberOperationOut output = await operation.Perform(input);
            });
        }

        /// <summary>
        /// Tests that the <see cref="ConvertToNumberOperation" /> operation uses a default value is a conversion fails.
        /// </summary>
        /// <param name="expectedValue">The expected converted number value.</param>
        /// <param name="defaultValue">The default number value.</param>
        /// <param name="text">The input string value.</param>
        [TestCase(1, 0, "1")]
        [TestCase(0, 0, "--3")]
        [TestCase(1, -1, "1e0")]
        [TestCase(3.14, 100, "3.14")]
        [TestCase(-5, -5, "abc")]
        [TestCase(0.14, 3.14, ".14")]
        [TestCase(-2.728, -2, "-2.728")]
        [TestCase(1.25, 1.25, "1.2.3")]
        [TestCase(1234, 0, "1.234e3")]
        [TestCase(0, 0, "4-")]
        [TestCase(-98765, 123, "-9.8765e4")]
        [TestCase(0, 0, null)]
        public async Task TestConvertWithDefault(double expectedValue, double defaultValue, string text)
        {
            ConvertToNumberOperation operation = new();
            ConvertToNumberOperationIn input = new() { Text = text, Default = defaultValue };
            ConvertToNumberOperationOut output = await operation.Perform(input);

            Assert.AreEqual(expectedValue, output.Number, 1e-4);
        }
    }
}