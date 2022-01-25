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
        [TestCase("123", 123)]
        [TestCase("0", 0)]
        [TestCase("3.14", 3.14)]
        [TestCase("-2.728", -2.728)]
        [TestCase("$123.46", 123.456, "C")]
        [TestCase("1,222,333.000", 1222333, "N")]
        [TestCase("96.700%", 0.967, "P")]
        public async Task TestConvertValid(string expectedValue, double number, string format = null)
        {
            ConvertToStringOperation operation = new();
            ConvertToStringOperationIn input = new() { Number = number, Format = format };
            ConvertToStringOperationOut output = await operation.Perform(input);

            Assert.AreEqual(expectedValue, output.Text);
        }

        // TODO: For now, we do not perform this test because of an unexpected behavior.
        //       See https://github.com/dotnet/runtime/issues/64295.
        /// <summary>
        /// Tests that the <see cref="ConvertToStringOperation" /> operation throws an exception when the format is not
        /// valid.
        // /// </summary>
        // /// <param name="number">The number value.</param>
        // /// <param name="format">An optional format specifier</param>
        // [TestCase(1234, ";;;")]
        // [TestCase(0, "hh")]
        // [TestCase(-3.14, "abc")]
        // public void TestConvertInvalid(double number, string format = null)
        // {
        //     Assert.ThrowsAsync<FormatException>(async () =>
        //     {
        //         ConvertToStringOperation operation = new();
        //         ConvertToStringOperationIn input = new() { Number = number, Format = format };
        //         ConvertToStringOperationOut output = await operation.Perform(input);
        //     });
        // }
    }
}