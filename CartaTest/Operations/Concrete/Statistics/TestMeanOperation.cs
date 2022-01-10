using System.Threading.Tasks;
using CartaCore.Operations;
using NUnit.Framework;

namespace CartaTest.Operations
{
    /// <summary>
    /// Tests the performance of the <see cref="MeanOperation" /> operation. 
    /// </summary>
    public class TestMeanOperation
    {
        /// <summary>
        /// Tests that the mean operation works as expected.
        /// </summary>
        /// <param name="expected">The expected mean.</param>
        /// <param name="values">The values to compute the mean of.</param>
        [TestCase(18.0, new double[] { 10, 12, 23, 23, 16, 23, 21, 16 })]
        [TestCase(0.0, new double[] { -1, 1, -2, 2 })]
        [TestCase(2.5, new double[] { 1, 2, 3, 4 })]
        [TestCase(42.0, new double[] { 42.0 })]
        [TestCase(double.NaN, new double[] { })]
        public async Task TestMean(double expected, double[] values)
        {
            MeanOperation operation = new();
            MeanOperationIn input = new() { Values = values };
            MeanOperationOut output = await operation.Perform(input);

            Assert.AreEqual(expected, output.Mean, 1.0e-4);
        }
    }
}