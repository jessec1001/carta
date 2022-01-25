using System;
using System.Threading.Tasks;
using CartaCore.Extensions.Statistics;
using CartaCore.Operations.Distribution;
using NUnit.Framework;

namespace CartaTest.Operations
{
    /// <summary>
    /// Tests the performance of the <see cref="SamplePoissonOperation" /> operation.
    /// </summary>
    public class TestSamplePoissonOperation
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
            double sampleMean = StatisticsExtensions.ComputeMean(samples);
            Assert.AreEqual(rate, sampleMean, 4 * Math.Sqrt((double)1 / sampleCount));
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
            double sampleDeviation = Math.Sqrt(StatisticsExtensions.ComputeMoment(samples, 2));
            Assert.AreEqual(Math.Sqrt(rate), sampleDeviation, 4 * Math.Sqrt((double)4 / sampleCount));
        }

        /// <summary>
        /// Tests that the random samples generated have approximately the expected skewness.
        /// </summary>
        /// <param name="sampleCount">The number of samples to generate.</param>
        /// <param name="seed">The seed used to seed the random operation.</param>
        /// <param name="rate">The rate parameter of the Poisson distribution.</param>
        [TestCase(14500, 45612, 1.0)]
        [TestCase(100000, 45612, 5.0)]
        [TestCase(2500, 45612, 10.0)]
        [TestCase(2500, 45612, 1.0)]
        [TestCase(1500, 45612, 0.2)]
        public async Task TestSkewness(int sampleCount, int seed, double rate)
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
            double sampleSkewness = StatisticsExtensions.ComputeNormalizedMoment(samples, 3);
            Assert.AreEqual(1.0 / Math.Sqrt(rate), sampleSkewness, 4 * Math.Sqrt((double)6 / sampleCount));
        }

        /// <summary>
        /// Tests that the random samples generated have approximately the expected kurtosis.
        /// </summary>
        /// <param name="sampleCount">The number of samples to generate.</param>
        /// <param name="seed">The seed used to seed the random operation.</param>
        /// <param name="rate">The rate parameter of the Poisson distribution.</param>
        [TestCase(14500, 45612, 1.0)]
        [TestCase(100000, 45612, 5.0)]
        [TestCase(2500, 45612, 10.0)]
        [TestCase(2500, 45612, 1.0)]
        public async Task TestKurtosis(int sampleCount, int seed, double rate)
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
            double sampleKurtosis = StatisticsExtensions.ComputeNormalizedMoment(samples, 4);
            Assert.AreEqual(3 + 1.0 / rate, sampleKurtosis, 4 * Math.Sqrt((double)24 / sampleCount));
        }

        /// <summary>
        /// Tests that the random samples are same if they are generated with the same seed.
        /// </summary>
        /// <param name="seed">The seed to use.</param>
        [TestCase(0)]
        [TestCase(10)]
        [TestCase(42)]
        [TestCase(-1234)]
        [TestCase(int.MaxValue)]
        public async Task TestSeededness(int seed)
        {
            SamplePoissonOperation operation = new();
            SamplePoissonOperationIn input = new()
            {
                Seed = seed,
                Rate = 1.0,
                Count = 1
            };
            SamplePoissonOperationOut output1 = await operation.Perform(input);
            SamplePoissonOperationOut output2 = await operation.Perform(input);

            Assert.AreEqual(output1.Samples, output2.Samples);
        }
    }
}