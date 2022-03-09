using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Extensions.Statistics;
using CartaCore.Operations.Distribution;
using NUnit.Framework;

namespace CartaTest.Operations
{
    /// <summary>
    /// Tests the performance of the <see cref="SamplePoissonOperation" /> operation.
    /// </summary>
    public class TestSampleUniformOperation
    {
        /// <summary>
        /// Tests that the random samples generated have approximately the expected mean.
        /// </summary>
        /// <param name="sampleCount">The number of samples to generate.</param>
        /// <param name="seed">The seed used to seed the random operation.</param>
        /// <param name="min">The minimum value of the uniform distribution.</param>
        /// <param name="max">The maximum value of the uniform distribution.</param>
        [TestCase(26500, 45612, 0.0, 1.0)]
        [TestCase(10000, 1, 1.0, 2.0)]
        [TestCase(100000, 10, 1.0, 2.0)]
        [TestCase(2500, 42, -10.0, 10.0)]
        [TestCase(2500, 42, 10.0, -10.0)]
        [TestCase(2500, 42, 0.0, 0.0)]
        public async Task TestMean(int sampleCount, int seed, double min, double max)
        {
            SampleUniformOperation operation = new();
            SampleUniformOperationIn input = new()
            {
                Seed = seed,
                Count = sampleCount,
                Minimum = min,
                Maximum = max,
            };
            SampleUniformOperationOut output = await operation.Perform(input);

            IEnumerable<double> samples = output.Samples;
            (double sampleMean, int _) = samples.ComputeRawMoment(1);
            Assert.AreEqual((min + max) / 2.0, sampleMean, 8 * Math.Sqrt((double)1 / sampleCount));
        }

        /// <summary>
        /// Tests that the random samples generated have approximately the expected deviation.
        /// </summary>
        /// <param name="sampleCount">The number of samples to generate.</param>
        /// <param name="seed">The seed used to seed the random operation.</param>
        /// <param name="min">The minimum value of the uniform distribution.</param>
        /// <param name="max">The maximum value of the uniform distribution.</param>
        [TestCase(26500, 45612, 0.0, 1.0)]
        [TestCase(10000, 1, 1.0, 2.0)]
        [TestCase(100000, 10, 1.0, 2.0)]
        [TestCase(2500, 42, -10.0, 10.0)]
        [TestCase(2500, 42, 10.0, -10.0)]
        [TestCase(2500, 42, 0.0, 0.0)]
        public async Task TestDeviation(int sampleCount, int seed, double min, double max)
        {
            SampleUniformOperation operation = new();
            SampleUniformOperationIn input = new()
            {
                Seed = seed,
                Count = sampleCount,
                Minimum = min,
                Maximum = max,
            };
            SampleUniformOperationOut output = await operation.Perform(input);

            IEnumerable<double> samples = output.Samples;
            (double sampleVariance, int _) = samples.ComputeCentralMoment(2);
            double sampleDeviation = Math.Sqrt(sampleVariance);
            Assert.AreEqual(Math.Abs(max - min) / Math.Sqrt(12.0), sampleDeviation, 4 * Math.Sqrt((double)4 / sampleCount));
        }

        /// <summary>
        /// Tests that the random samples generated have approximately the expected skewness.
        /// </summary>
        /// <param name="sampleCount">The number of samples to generate.</param>
        /// <param name="seed">The seed used to seed the random operation.</param>
        /// <param name="min">The minimum value of the uniform distribution.</param>
        /// <param name="max">The maximum value of the uniform distribution.</param>
        [TestCase(26500, 45612, 0.0, 1.0)]
        [TestCase(10000, 1, 1.0, 2.0)]
        [TestCase(100000, 10, 1.0, 2.0)]
        [TestCase(2500, 42, -10.0, 10.0)]
        [TestCase(2500, 42, 10.0, -10.0)]
        public async Task TestSkewness(int sampleCount, int seed, double min, double max)
        {
            SampleUniformOperation operation = new();
            SampleUniformOperationIn input = new()
            {
                Seed = seed,
                Count = sampleCount,
                Minimum = min,
                Maximum = max,
            };
            SampleUniformOperationOut output = await operation.Perform(input);

            IEnumerable<double> samples = output.Samples;
            (double sampleSkewness, int _) = samples.ComputeNormalizedMoment(3);
            Assert.AreEqual(0.0, sampleSkewness, 4 * Math.Sqrt((double)6 / sampleCount));
        }

        /// <summary>
        /// Tests that the random samples generated have approximately the expected kurtosis.
        /// </summary>
        /// <param name="sampleCount">The number of samples to generate.</param>
        /// <param name="seed">The seed used to seed the random operation.</param>
        /// <param name="min">The minimum value of the uniform distribution.</param>
        /// <param name="max">The maximum value of the uniform distribution.</param>
        [TestCase(26500, 45612, 0.0, 1.0)]
        [TestCase(10000, 1, 1.0, 2.0)]
        [TestCase(100000, 10, 1.0, 2.0)]
        [TestCase(2500, 42, -10.0, 10.0)]
        [TestCase(2500, 42, 10.0, -10.0)]
        public async Task TestKurtosis(int sampleCount, int seed, double min, double max)
        {
            SampleUniformOperation operation = new();
            SampleUniformOperationIn input = new()
            {
                Seed = seed,
                Count = sampleCount,
                Minimum = min,
                Maximum = max,
            };
            SampleUniformOperationOut output = await operation.Perform(input);

            IEnumerable<double> samples = output.Samples;
            (double sampleKurtosis, int _) = samples.ComputeNormalizedMoment(4);
            Assert.AreEqual(3.0 - 6.0 / 5.0, sampleKurtosis, 4 * Math.Sqrt((double)24 / sampleCount));
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
            SampleUniformOperation operation = new();
            SampleUniformOperationIn input = new()
            {
                Seed = seed,
                Count = 1,
                Minimum = 0.0,
                Maximum = 1.0,
            };
            SampleUniformOperationOut output1 = await operation.Perform(input);
            SampleUniformOperationOut output2 = await operation.Perform(input);

            Assert.AreEqual(output1.Samples, output2.Samples);
        }
    }
}