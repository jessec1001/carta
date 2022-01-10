using System.Threading.Tasks;
using CartaCore.Operations;
using NUnit.Framework;

namespace CartaTest.Operations
{
    /// <summary>
    /// Tests the performance of the <see cref="DeviationOperation" /> operation.
    /// </summary>
    public class TestDeviationOperation
    {
        /// <summary>
        /// Tests that the deviation operation works as expected without Bessel's correction.
        /// </summary>
        /// <param name="expected">The expected deviation.</param>
        /// <param name="values">The values to compute the deviation of.</param>
        [TestCase(0.8164965809277, new double[] { 1, 2, 3 })]
        [TestCase(4.8989794855664, new double[] { 10, 12, 23, 23, 16, 23, 21, 16 })]
        [TestCase(1.0000000000000, new double[] { -1, 1, -1, 1, -1, 1 })]
        [TestCase(0.0000000000000, new double[] { 1 })]
        [TestCase(double.NaN, new double[] { })]
        public async Task TestDeviationWithoutBessel(double expected, double[] values)
        {
            DeviationOperation operation = new();
            DeviationOperationIn input = new() { Values = values, UseBesselsCorrection = false };
            DeviationOperationOut output = await operation.Perform(input);

            Assert.AreEqual(expected, output.Deviation, 1.0e-4);
        }

        /// <summary>
        /// Tests that the deviation operation works as expected with Bessel's correction.
        /// </summary>
        /// <param name="expected">The expected deviation.</param>
        /// <param name="values">The values to compute the deviation of.</param>
        [TestCase(1.0000000000000, new double[] { 1, 2, 3 })]
        [TestCase(5.2372293656638, new double[] { 10, 12, 23, 23, 16, 23, 21, 16 })]
        [TestCase(1.0954451150103, new double[] { -1, 1, -1, 1, -1, 1 })]
        [TestCase(double.NaN, new double[] { 1 })]
        [TestCase(double.NaN, new double[] { })]
        public async Task TestDeviationWithBessel(double expected, double[] values)
        {
            DeviationOperation operation = new();
            DeviationOperationIn input = new() { Values = values, UseBesselsCorrection = true };
            DeviationOperationOut output = await operation.Perform(input);

            Assert.AreEqual(expected, output.Deviation, 1.0e-4);
        }
    }
}