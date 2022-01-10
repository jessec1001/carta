using System;
using System.Threading.Tasks;
using CartaCore.Operations;
using CartaCore.Statistics;
using NUnit.Framework;

namespace CartaTest.Operations
{
    /// <summary>
    /// Tests the performance of the <see cref="SamplePoissonOperation" /> operation.
    /// </summary>
    public class TestRandomPoissonOperation
    {
        /// <summary>
        /// Tests that the random samples generated have approximately the correct mean as specified in the input.
        /// </summary>
        /// <param name="sampleCount">The number of samples to generate.</param>
        /// <param name="seed">The seed used to seed the random operation.</param>
        /// <param name="rate">The rate parameter of the Poisson distribution.</param>
        [TestCase(14500, 45612, 1.0)]
        [TestCase(100000, 45612, 5.0)]
        [TestCase(2500, 45612, 10.0)]
        [TestCase(2500, 45612, 1.0)]
        [TestCase(2500, 45612, 0.2)]
        public async Task TestMean(int sampleCount, int seed, double rate)
        {
            SamplePoissonOperation operation = new();
            SamplePoissonOperationIn input = new()
            {
                Seed = seed,
                Rate = rate,
                Count = sampleCount
            };
            SamplePoissonOperationOut output = await operation.Perform(input);

            double[] samples = Array.ConvertAll<int, double>(output.Samples, x => x);
            double sampleMean = StatisticsUtility.ComputeMean(samples);
            Assert.AreEqual(rate, sampleMean, Math.Pow(sampleCount, -0.25));
        }
        /// <summary>
        /// Tests that the random samples generated have approximately the correct deviation as specified in the input.
        /// </summary>
        /// <param name="sampleCount">The number of samples to generate.</param>
        /// <param name="seed">The seed used to seed the random operation.</param>
        /// <param name="rate">The rate parameter of the Poisson distribution.</param>
        [TestCase(14500, 45612, 1.0)]
        [TestCase(100000, 45612, 5.0)]
        [TestCase(2500, 45612, 10.0)]
        [TestCase(2500, 45612, 1.0)]
        [TestCase(2500, 45612, 0.2)]
        public async Task TestDeviation(int sampleCount, int seed, double rate)
        {
            SamplePoissonOperation operation = new();
            SamplePoissonOperationIn input = new()
            {
                Seed = seed,
                Rate = rate,
                Count = sampleCount
            };
            SamplePoissonOperationOut output = await operation.Perform(input);

            double[] samples = Array.ConvertAll<int, double>(output.Samples, x => x);
            double sampleDeviation = Math.Sqrt(StatisticsUtility.ComputeMoment(samples, 2));
            Assert.AreEqual(Math.Sqrt(rate), sampleDeviation, Math.Pow(sampleCount, -0.20));
        }
    }
}