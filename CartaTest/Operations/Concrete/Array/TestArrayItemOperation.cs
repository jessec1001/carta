using System;
using System.Threading.Tasks;
using CartaCore.Operations.Array;
using NUnit.Framework;

namespace CartaTest.Operations
{
    // TODO: Make it so that the array item operation has better type safety so that
    //  the input array element type is the same as the output type when converting or checking.

    /// <summary>
    /// Tests the performance of the <see cref="ArrayItemOperation" /> operation.
    /// </summary>
    public class TestArrayItemOperation
    {
        /// <summary>
        /// Tests that the <see cref="ArrayItemOperation" /> returns the correct item from an array at a valid index.
        /// </summary>
        /// <param name="expected">The expected item.</param>
        /// <param name="value">The array value.</param>
        /// <param name="index">The index of the array. Can be negative.</param>
        [TestCase(1, new object[] { 1 }, 0)]
        [TestCase(1, new object[] { 1 }, -1)]
        [TestCase(true, new object[] { true, false, true, false }, 2)]
        [TestCase("d", new object[] { "a", "b", "c", "d", "e", "f" }, 3)]
        [TestCase("d", new object[] { "a", "b", "c", "d", "e", "f" }, -3)]
        public async Task TestValidIndex(object expected, object[] value, int index)
        {
            ArrayItemOperation operation = new();
            ArrayItemOperationIn input = new() { Items = value, Index = index };
            ArrayItemOperationOut output = await operation.Perform(input);

            Assert.AreEqual(expected, output.Item);
        }
        /// <summary>
        /// Tests that the <see cref="ArrayItemOperation" /> throws an exception when the index is out of bounds.
        /// </summary>
        /// <param name="value">The array value.</param>
        /// <param name="index">The index of the array. Can be negative.</param>
        [TestCase(new object[] { }, 0)]
        [TestCase(new object[] { }, -1)]
        [TestCase(new object[] { 1 }, 1)]
        [TestCase(new object[] { true, false, true, false }, 20)]
        [TestCase(new object[] { "a", "b", "c", "d", "e", "f" }, -7)]
        public void TestInvalidIndex(object[] value, int index)
        {
            Assert.ThrowsAsync<IndexOutOfRangeException>(async () =>
            {
                ArrayItemOperation operation = new();
                ArrayItemOperationIn input = new() { Items = value, Index = index };
                ArrayItemOperationOut output = await operation.Perform(input);
            });
        }

        /// <summary>
        /// Tests that the <see cref="ArrayItemOperation" /> throws an exception when the array is null.
        /// </summary>
        /// <param name="index">The index of the array. Can be negative.</param>
        [TestCase(0)]
        [TestCase(-2)]
        [TestCase(3)]
        public void TestNullArray(int index)
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                ArrayItemOperation operation = new();
                ArrayItemOperationIn input = new() { Items = null, Index = index };
                ArrayItemOperationOut output = await operation.Perform(input);
            });
        }
    }
}