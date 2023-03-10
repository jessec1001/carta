using System;
using System.Threading.Tasks;
using CartaCore.Operations.Conversion;
using NUnit.Framework;

namespace CartaTest.Operations
{
    /// <summary>
    /// Tests the performance of the <see cref="ConvertToStringOperation" /> operation.
    /// </summary>
    public class TestConvertToStringOperation
    {
        /// <summary>
        /// Tests that the <see cref="ConvertToStringOperation" /> operation converts valid number formats correctly.
        /// </summary>
        /// <param name="expectedValue">The expected converted string value.</param>
        /// <param name="number">The number value.</param>
        /// <param name="format">An optional format specifier.</param>
        [SetCulture("en-US")]
        [TestCase("123", 123)]
        [TestCase("0", 0)]
        [TestCase("3.14", 3.14)]
        [TestCase("-2.728", -2.728)]
        [TestCase("1,222,333.000", 1222333, "N3")]
        [TestCase("96.70%", 0.967, "P2")]
        public async Task TestConvertValid(string expectedValue, double number, string format = null)
        {
            ConvertToStringOperation operation = new();
            ConvertToStringOperationIn input = new() { Number = number, Format = format };
            ConvertToStringOperationOut output = await operation.Perform(input);

            Assert.AreEqual(expectedValue, output.Text);
        }
    }
}