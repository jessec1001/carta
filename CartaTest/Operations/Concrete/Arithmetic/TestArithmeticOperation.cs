using System.Threading.Tasks;
using CartaCore.Operations;
using NUnit.Framework;

namespace CartaTest.Operations
{
    /// <summary>
    /// Tests the performance of the <see cref="ArithmeticOperation" /> operation.
    /// </summary>
    public class TestArithmeticOperation
    {
        /// <summary>
        /// Tests that the <see cref="ArithmeticOperation" /> operation handles addition correctly.
        /// </summary>
        /// <param name="expected">The expected sum.</param>
        /// <param name="value1">The first operand.</param>
        /// <param name="value2">The second operand.</param>
        [TestCase(0.0, 0.0, 0.0)]
        [TestCase(3.0, 1.0, 2.0)]
        [TestCase(0.0, -1.0, +1.0)]
        [TestCase(double.PositiveInfinity, double.PositiveInfinity, -10.0)]
        [TestCase(double.NaN, double.NaN, 3.0)]
        [TestCase(0.422, 3.14, -2.718)]
        public async Task TestAddition(double expected, double value1, double value2)
        {
            ArithmeticOperation operation = new();
            ArithmeticOperationIn input = new()
            {
                Type = ArithmeticOperationType.Add,
                Input1 = value1,
                Input2 = value2
            };
            ArithmeticOperationOut output = await operation.Perform(input);

            Assert.AreEqual(expected, output.Output, 1e-4);
        }

        /// <summary>
        /// Tests that the <see cref="ArithmeticOperation" /> operation handles subtraction correctly.
        /// </summary>
        /// <param name="expected">The expected difference.</param>
        /// <param name="value1">The first operand.</param>
        /// <param name="value2">The second operand.</param>
        [TestCase(0.0, 0.0, 0.0)]
        [TestCase(-1.0, 1.0, 2.0)]
        [TestCase(-2.0, -1.0, +1.0)]
        [TestCase(double.PositiveInfinity, double.PositiveInfinity, -10.0)]
        [TestCase(double.NaN, double.NaN, 3.0)]
        [TestCase(0.422, 3.14, 2.718)]
        public async Task TestSubtraction(double expected, double value1, double value2)
        {
            ArithmeticOperation operation = new();
            ArithmeticOperationIn input = new()
            {
                Type = ArithmeticOperationType.Subtract,
                Input1 = value1,
                Input2 = value2
            };
            ArithmeticOperationOut output = await operation.Perform(input);

            Assert.AreEqual(expected, output.Output, 1e-4);
        }

        /// <summary>
        /// Tests that the <see cref="ArithmeticOperation" /> operation handles multiplication correctly.
        /// </summary>
        /// <param name="expected">The expected product.</param>
        /// <param name="value1">The first operand.</param>
        /// <param name="value2">The second operand.</param>
        [TestCase(0.0, 0.0, 0.0)]
        [TestCase(2.0, 1.0, 2.0)]
        [TestCase(-1.0, -1.0, +1.0)]
        [TestCase(double.NegativeInfinity, double.PositiveInfinity, -10.0)]
        [TestCase(double.NaN, double.NaN, 3.0)]
        [TestCase(8.53452, 3.14, 2.718)]
        public async Task TestMultiplication(double expected, double value1, double value2)
        {
            ArithmeticOperation operation = new();
            ArithmeticOperationIn input = new()
            {
                Type = ArithmeticOperationType.Multiply,
                Input1 = value1,
                Input2 = value2
            };
            ArithmeticOperationOut output = await operation.Perform(input);

            Assert.AreEqual(expected, output.Output, 1e-4);
        }

        /// <summary>
        /// Tests that the <see cref="ArithmeticOperation" /> operation handles division correctly.
        /// </summary>
        /// <param name="expected">The expected quotient.</param>
        /// <param name="value1">The first operand.</param>
        /// <param name="value2">The second operand.</param>
        [TestCase(double.PositiveInfinity, 1.0, 0.0)]
        [TestCase(double.NegativeInfinity, -1.0, 0.0)]
        [TestCase(0.5, 1.0, 2.0)]
        [TestCase(-1.0, -1.0, +1.0)]
        [TestCase(double.PositiveInfinity, double.PositiveInfinity, -10.0)]
        [TestCase(double.NaN, double.NaN, 3.0)]
        [TestCase(1.15526122, 3.14, 2.718)]
        public async Task TestDivision(double expected, double value1, double value2)
        {
            ArithmeticOperation operation = new();
            ArithmeticOperationIn input = new()
            {
                Type = ArithmeticOperationType.Divide,
                Input1 = value1,
                Input2 = value2
            };
            ArithmeticOperationOut output = await operation.Perform(input);

            Assert.AreEqual(expected, output.Output, 1e-4);
        }

        /// <summary>
        /// Tests that the <see cref="ArithmeticOperation" /> operation handles exponentiation correctly.
        /// </summary>
        /// <param name="expected">The expected output.</param>
        /// <param name="value1">The first operand.</param>
        /// <param name="value2">The second operand.</param>
        [TestCase(1.0, 0.0, 0.0)]
        [TestCase(4.0, 2.0, 2.0)]
        [TestCase(1.0, -1.0, +2.0)]
        [TestCase(0.0, double.PositiveInfinity, -10.0)]
        [TestCase(double.NaN, double.NaN, 3.0)]
        [TestCase(0.0446010637, 3.14, -2.718)]
        public async Task TestExponentiation(double expected, double value1, double value2)
        {
            ArithmeticOperation operation = new();
            ArithmeticOperationIn input = new()
            {
                Type = ArithmeticOperationType.Exponentiate,
                Input1 = value1,
                Input2 = value2
            };
            ArithmeticOperationOut output = await operation.Perform(input);

            Assert.AreEqual(expected, output.Output, 1e-4);
        }
    }
}