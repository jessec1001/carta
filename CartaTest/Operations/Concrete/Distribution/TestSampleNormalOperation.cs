using System;
using System.Threading.Tasks;
using CartaCore.Extensions.Statistics;
using CartaCore.Operations.Distribution;
using NUnit.Framework;

namespace CartaTest.Operations
{
    /// <summary>
    /// Tests the performance of the <see cref="SampleNormalOperation" /> operation.
    /// </summary>
    public class TestSampleNormalOperation
    {
        /// <summary>
        /// Tests that the random samples that are generated are normally distributed.
        /// Uses the [Jarque-Bera Test](https://en.wikipedia.org/wiki/Jarque%E2%80%93Bera_test).
        /// </summary>
        /// <param name="sampleCount">The number of samples to generate.</param>
        /// <param name="seed">The seed used to seed the random operation.</param>
        /// <param name="mean">The mean of the normal distribution.</param>
        /// <param name="deviation">The deviation of the normal distribution.</param>
        [TestCase(26500, 45612, -1.0, 2.0)]
        [TestCase(10000, 1, 1.0, 2.0)]
        [TestCase(100000, 10, 0.0, 1.0)]
        [TestCase(2500, 42, 0.0, 10.0)]
        [TestCase(2500, 42, 0.0, 0.2)]
        [TestCase(2500, 42, 10.0, 1.0)]
        public async Task TestNormality(int sampleCount, int seed, double mean, double deviation)
        {
            SampleNormalOperation operation = new();
            SampleNormalOperationIn input = new()
            {
                Seed = seed,
                Mean = mean,
                Deviation = deviation,
                Count = sampleCount
            };
            SampleNormalOperationOut output = await operation.Perform(input);

            // Test the normality of the samples.
            double jbStatistic = StatisticsExtensions.ComputeJarqueBera(output.Samples);
            Assert.AreEqual(0.0, jbStatistic, 10.0);
        }

        /// <summary>
        /// Tests that the random samples generated have approximately the same mean as specified in the input.
        /// </summary>
        /// <param name="sampleCount">The number of samples to generate.</param>
        /// <param name="seed">The seed used to seed the random operation.</param>
        /// <param name="mean">The mean of the normal distribution.</param>
        /// <param name="deviation">The deviation of the normal distribution.</param>
        [TestCase(26500, 45612, -1.0, 2.0)]
        [TestCase(10000, 1, 1.0, 2.0)]
        [TestCase(100000, 10, 0.0, 1.0)]
        [TestCase(2500, 42, 0.0, 10.0)]
        [TestCase(2500, 42, 0.0, 0.2)]
        [TestCase(2500, 42, 10.0, 1.0)]
        public async Task TestMean(int sampleCount, int seed, double mean, double deviation)
        {
            SampleNormalOperation operation = new();
            SampleNormalOperationIn input = new()
            {
                Seed = seed,
                Mean = mean,
                Deviation = deviation,
                Count = sampleCount
            };
            SampleNormalOperationOut output = await operation.Perform(input);

            double sampleMean = StatisticsExtensions.ComputeMean(output.Samples);
            Assert.AreEqual(mean, sampleMean, deviation * Math.Sqrt((double)1 / sampleCount));
        }
        /// <summary>
        /// Tests that the random samples generated have approximately the same deviation as specified in the input.
        /// </summary>
        /// <param name="sampleCount">The number of samples to generate.</param>
        /// <param name="seed">The seed used to seed the random operation.</param>
        /// <param name="mean">The mean of the normal distribution.</param>
        /// <param name="deviation">The deviation of the normal distribution.</param>
        [TestCase(26500, 45612, -1.0, 2.0)]
        [TestCase(10000, 1, 1.0, 2.0)]
        [TestCase(100000, 10, 0.0, 1.0)]
        [TestCase(2500, 42, 0.0, 10.0)]
        [TestCase(2500, 42, 0.0, 0.2)]
        [TestCase(2500, 42, 10.0, 1.0)]
        public async Task TestDeviation(int sampleCount, int seed, double mean, double deviation)
        {
            SampleNormalOperation operation = new();
            SampleNormalOperationIn input = new()
            {
                Seed = seed,
                Mean = mean,
                Deviation = deviation,
                Count = sampleCount
            };
            SampleNormalOperationOut output = await operation.Perform(input);

            double sampleDeviation = Math.Sqrt(StatisticsExtensions.ComputeMoment(output.Samples, 2));
            Assert.AreEqual(deviation, sampleDeviation, 4 * Math.Sqrt((double)4 / sampleCount));
        }

        /// <summary>
        /// Tests that the random samples generated have approximately the expected skewness.
        /// </summary>
        /// <param name="sampleCount">The number of samples to generate.</param>
        /// <param name="seed">The seed used to seed the random operation.</param>
        /// <param name="mean">The mean of the normal distribution.</param>
        /// <param name="deviation">The deviation of the normal distribution.</param>
        [TestCase(26500, 45612, -1.0, 2.0)]
        [TestCase(10000, 1, 1.0, 2.0)]
        [TestCase(100000, 10, 0.0, 1.0)]
        [TestCase(2500, 42, 0.0, 10.0)]
        [TestCase(2500, 42, 0.0, 0.2)]
        [TestCase(2500, 42, 10.0, 1.0)]
        public async Task TestSkewness(int sampleCount, int seed, double mean, double deviation)
        {
            SampleNormalOperation operation = new();
            SampleNormalOperationIn input = new()
            {
                Seed = seed,
                Mean = mean,
                Deviation = deviation,
                Count = sampleCount
            };
            SampleNormalOperationOut output = await operation.Perform(input);

            double sampleSkewness = StatisticsExtensions.ComputeNormalizedMoment(output.Samples, 3);
            Assert.AreEqual(0.0, sampleSkewness, 4 * Math.Sqrt((double)6 / sampleCount));
        }

        /// <summary>
        /// Tests that the random samples generated have approximately the expected kurtosis.
        /// </summary>
        /// <param name="sampleCount">The number of samples to generate.</param>
        /// <param name="seed">The seed used to seed the random operation.</param>
        /// <param name="mean">The mean of the normal distribution.</param>
        /// <param name="deviation">The deviation of the normal distribution.</param>
        [TestCase(26500, 45612, -1.0, 2.0)]
        [TestCase(10000, 1, 1.0, 2.0)]
        [TestCase(100000, 10, 0.0, 1.0)]
        [TestCase(2500, 42, 0.0, 10.0)]
        [TestCase(2500, 42, 0.0, 0.2)]
        [TestCase(2500, 42, 10.0, 1.0)]
        public async Task TestKurtosis(int sampleCount, int seed, double mean, double deviation)
        {
            SampleNormalOperation operation = new();
            SampleNormalOperationIn input = new()
            {
                Seed = seed,
                Mean = mean,
                Deviation = deviation,
                Count = sampleCount
            };
            SampleNormalOperationOut output = await operation.Perform(input);

            double sampleKurtosis = StatisticsExtensions.ComputeNormalizedMoment(output.Samples, 4);
            Assert.AreEqual(3.0, sampleKurtosis, 4 * Math.Sqrt((double)24 / sampleCount));
        }

        /// <summary>
        /// Tests that random samples are same if they are generated with the same seed.
        /// </summary>
        /// <param name="seed">The seed to use.</param>
        [TestCase(0)]
        [TestCase(10)]
        [TestCase(42)]
        [TestCase(-1234)]
        [TestCase(int.MaxValue)]
        public async Task TestSeededness(int seed)
        {
            SampleNormalOperation operation = new();
            SampleNormalOperationIn input = new()
            {
                Seed = seed,
                Mean = 0.0,
                Deviation = 1.0,
                Count = 1
            };
            SampleNormalOperationOut output1 = await operation.Perform(input);
            SampleNormalOperationOut output2 = await operation.Perform(input);

            Assert.AreEqual(output1.Samples, output2.Samples);
        }
    }
}