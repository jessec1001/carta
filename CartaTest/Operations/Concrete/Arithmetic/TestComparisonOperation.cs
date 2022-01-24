using System.Threading.Tasks;
using CartaCore.Operations.Arithmetic;
using NUnit.Framework;

namespace CartaTest.Operations
{
    /// <summary>
    /// Tests the performance of the <see cref="ComparisonOperation" /> operation.
    /// </summary>
    public class TestComparisonOperation
    {
        /// <summary>
        /// Tests that the <see cref="ComparisonOperation" /> operation handles the less than comparison correctly.
        /// </summary>
        /// <param name="expected">The expected result.</param>
        /// <param name="value1">The first operand.</param>
        /// <param name="value2">The second operand.</param>
        [TestCase(true, 1.0, 2.0)]
        [TestCase(true, -1.0, 2.0)]
        [TestCase(false, 2.0, 1.0)]
        [TestCase(true, 4.0, double.PositiveInfinity)]
        [TestCase(false, -4.0, double.NegativeInfinity)]
        [TestCase(false, 20.0, 20.0)]
        [TestCase(true, 2.718, 3.14)]
        [TestCase(false, 5.5, double.NaN)]
        public async Task TestLessThan(bool expected, double value1, double value2)
        {
            ComparisonOperation operation = new();
            ComparisonOperationIn input = new()
            {
                Type = ComparisonOperationType.LessThan,
                Input1 = value1,
                Input2 = value2
            };
            ComparisonOperationOut output = await operation.Perform(input);

            Assert.AreEqual(expected, output.Output);
        }

        /// <summary>
        /// Tests that the <see cref="ComparisonOperation" /> operation handles the less than or equal to comparison
        /// correctly.
        /// </summary>
        /// <param name="expected">The expected result.</param>
        /// <param name="value1">The first operand.</param>
        /// <param name="value2">The second operand.</param>
        [TestCase(true, 1.0, 2.0)]
        [TestCase(true, -1.0, 2.0)]
        [TestCase(false, 2.0, 1.0)]
        [TestCase(true, 4.0, double.PositiveInfinity)]
        [TestCase(false, -4.0, double.NegativeInfinity)]
        [TestCase(true, 20.0, 20.0)]
        [TestCase(true, 2.718, 3.14)]
        [TestCase(false, 5.5, double.NaN)]
        public async Task TestLessThanOrEqualTo(bool expected, double value1, double value2)
        {
            ComparisonOperation operation = new();
            ComparisonOperationIn input = new()
            {
                Type = ComparisonOperationType.LessThanOrEqualTo,
                Input1 = value1,
                Input2 = value2
            };
            ComparisonOperationOut output = await operation.Perform(input);

            Assert.AreEqual(expected, output.Output);
        }

        /// <summary>
        /// Tests that the <see cref="ComparisonOperation" /> operation handles the greater than comparison correctly.
        /// </summary>
        /// <param name="expected">The expected result.</param>
        /// <param name="value1">The first operand.</param>
        /// <param name="value2">The second operand.</param>
        [TestCase(false, 1.0, 2.0)]
        [TestCase(false, -1.0, 2.0)]
        [TestCase(true, 2.0, 1.0)]
        [TestCase(false, 4.0, double.PositiveInfinity)]
        [TestCase(true, -4.0, double.NegativeInfinity)]
        [TestCase(false, 20.0, 20.0)]
        [TestCase(false, 2.718, 3.14)]
        [TestCase(false, 5.5, double.NaN)]
        public async Task TestGreaterThan(bool expected, double value1, double value2)
        {
            ComparisonOperation operation = new();
            ComparisonOperationIn input = new()
            {
                Type = ComparisonOperationType.GreaterThan,
                Input1 = value1,
                Input2 = value2
            };
            ComparisonOperationOut output = await operation.Perform(input);

            Assert.AreEqual(expected, output.Output);
        }

        /// <summary>
        /// Tests that the <see cref="ComparisonOperation" /> operation handles the greater than or equal to comparison
        /// correctly.
        /// </summary>
        /// <param name="expected">The expected result.</param>
        /// <param name="value1">The first operand.</param>
        /// <param name="value2">The second operand.</param>
        [TestCase(false, 1.0, 2.0)]
        [TestCase(false, -1.0, 2.0)]
        [TestCase(true, 2.0, 1.0)]
        [TestCase(false, 4.0, double.PositiveInfinity)]
        [TestCase(true, -4.0, double.NegativeInfinity)]
        [TestCase(true, 20.0, 20.0)]
        [TestCase(false, 2.718, 3.14)]
        [TestCase(false, 5.5, double.NaN)]
        public async Task TestGreaterThanOrEqualTo(bool expected, double value1, double value2)
        {
            ComparisonOperation operation = new();
            ComparisonOperationIn input = new()
            {
                Type = ComparisonOperationType.GreaterThanOrEqualTo,
                Input1 = value1,
                Input2 = value2
            };
            ComparisonOperationOut output = await operation.Perform(input);

            Assert.AreEqual(expected, output.Output);
        }

        /// <summary>
        /// Tests that the <see cref="ComparisonOperation" /> operation handles the equality comparison correctly.
        /// </summary>
        /// <param name="expected">The expected result.</param>
        /// <param name="value1">The first operand.</param>
        /// <param name="value2">The second operand.</param>
        [TestCase(false, 3.14, 2.718)]
        [TestCase(false, 4.0, double.PositiveInfinity)]
        [TestCase(false, -4.0, double.NegativeInfinity)]
        [TestCase(true, 20.0, 20.0)]
        [TestCase(true, double.PositiveInfinity, double.PositiveInfinity)]
        [TestCase(false, double.NaN, double.NaN)]
        public async Task TestEqualTo(bool expected, double value1, double value2)
        {
            ComparisonOperation operation = new();
            ComparisonOperationIn input = new()
            {
                Type = ComparisonOperationType.EqualTo,
                Input1 = value1,
                Input2 = value2
            };
            ComparisonOperationOut output = await operation.Perform(input);

            Assert.AreEqual(expected, output.Output);
        }

        /// <summary>
        /// Tests that the <see cref="ComparisonOperation" /> operation handles the inequality comparison correctly.
        /// </summary>
        /// <param name="expected">The expected result.</param>
        /// <param name="value1">The first operand.</param>
        /// <param name="value2">The second operand.</param>
        [TestCase(true, 3.14, 2.718)]
        [TestCase(true, 4.0, double.PositiveInfinity)]
        [TestCase(true, -4.0, double.NegativeInfinity)]
        [TestCase(false, 20.0, 20.0)]
        [TestCase(false, double.PositiveInfinity, double.PositiveInfinity)]
        [TestCase(true, double.NaN, double.NaN)]
        public async Task TestNotEqualTo(bool expected, double value1, double value2)
        {
            ComparisonOperation operation = new();
            ComparisonOperationIn input = new()
            {
                Type = ComparisonOperationType.NotEqualTo,
                Input1 = value1,
                Input2 = value2
            };
            ComparisonOperationOut output = await operation.Perform(input);

            Assert.AreEqual(expected, output.Output);
        }
    }
}