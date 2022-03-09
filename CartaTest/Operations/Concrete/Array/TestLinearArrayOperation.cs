using System.Threading.Tasks;
using CartaCore.Operations.Arrays;
using NUnit.Framework;

namespace CartaTest.Operations
{
    /// <summary>
    /// Tests the performance of the <see cref="LinearArrayOperation" /> operation.
    /// </summary>
    public class TestLinearArrayOperation
    {
        /// <summary>
        /// Tests that the <see cref="LinearArrayOperation" /> operation generates the correct array of values.
        /// </summary>
        /// <param name="expected">The expected array value.</param>
        /// <param name="min">The minimum value in the range.</param>
        /// <param name="max">The maximum value in the range.</param>
        /// <param name="steps">The number of steps in the range.</param>
        /// <param name="exclusiveMin">Whether the minimum value should be exclusive.</param>
        /// <param name="exclusiveMax">Whether the maximum value should be exclusive.</param>
        [TestCase(new double[] { 1.5, 1.5, 1.5 }, 1.5, 1.5, 3)]
        [TestCase(new double[] { 1.5, 1.5, 1.5 }, 1.5, 1.5, 3, true, false)]
        [TestCase(new double[] { 1, 2, 3, 4, 5 }, 1, 5, 5)]
        [TestCase(new double[] { 1, 2, 3, 4 }, 1, 5, 4, false, true)]
        [TestCase(new double[] { -1, -2, -3, -4, -5 }, 0, -5, 5, true, false)]
        [TestCase(new double[] { 5, 4, 3, 2, 1 }, 1, 5, -5)]
        public async Task TestArrayValues(
            double[] expected,
            double min,
            double max,
            int steps,
            bool exclusiveMin = false,
            bool exclusiveMax = false)
        {
            LinearArrayOperation operation = new();
            LinearArrayOperationIn input = new()
            {
                Minimum = min,
                Maximum = max,
                Steps = steps,
                ExclusiveMinimum = exclusiveMin,
                ExclusiveMaximum = exclusiveMax
            };
            LinearArrayOperationOut output = await operation.Perform(input);

            Assert.AreEqual(expected, output.Values);
        }
    }
}