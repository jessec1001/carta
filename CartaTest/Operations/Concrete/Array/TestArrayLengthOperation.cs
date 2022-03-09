using System.Threading.Tasks;
using CartaCore.Operations.Arrays;
using NUnit.Framework;

namespace CartaTest.Operations
{
    /// <summary>
    /// Tests the performance of the <see cref="ArrayLengthOperation" /> operation.
    /// </summary>
    public class TestArrayLengthOperation
    {
        /// <summary>
        /// Tests that the <see cref="ArrayLengthOperation" /> operation handles a non-null array correctly.
        /// </summary>
        /// <param name="expected">The expected length.</param>
        /// <param name="value">The array value.</param>
        [TestCase(0, new object[] { })]
        [TestCase(1, new object[] { 1 })]
        [TestCase(2, new object[] { 1, 2 })]
        [TestCase(3, new object[] { 1, 2, 3 })]
        [TestCase(4, new object[] { "apples", double.NaN, false, 0x13 })]
        public async Task TestArrayLength(int expected, object[] value)
        {
            ArrayLengthOperation operation = new();
            ArrayLengthOperationIn input = new() { Items = value };
            ArrayLengthOperationOut output = await operation.Perform(input);

            Assert.AreEqual(expected, output.Length);
        }

        /// <summary>
        /// Tests that the <see cref="ArrayLengthOperation" /> operation handles a null array correctly.
        /// </summary>
        [Test]
        public async Task TestNullArray()
        {
            ArrayLengthOperation operation = new();
            ArrayLengthOperationIn input = new() { Items = null };
            ArrayLengthOperationOut output = await operation.Perform(input);

            Assert.AreEqual(0, output.Length);
        }
    }
}