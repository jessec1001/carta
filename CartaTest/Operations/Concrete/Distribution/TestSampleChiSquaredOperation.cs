using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Extensions.Statistics;
using CartaCore.Operations.Distribution;
using NUnit.Framework;

namespace CartaTest.Operations
{
    /// <summary>
    /// Tests the performance of the <see cref="SampleChiSquaredOperation" /> operation.
    /// </summary>
    public class TestSampleChiSquaredOperation
    {
        /// <summary>
        /// Tests that the random samples generated have approximately the expected mean.
        /// </summary>
        /// <param name="sampleCount">The number of samples to generate.</param>
        /// <param name="seed">The seed used to seed the random operation.</param>
        /// <param name="degreesOfFreedom">The degrees of freedom parameter of the Chi-Squared distribution.</param>
        [TestCase(14500, 45612, 2)]
        [TestCase(100000, 45612, 2)]
        [TestCase(25000, 45612, 2)]
        [TestCase(25000, 45612, 5)]
        [TestCase(25000, 45612, 20)]
        public async Task TestMean(int sampleCount, int seed, int degreesOfFreedom)
        {
            SampleChiSquaredOperation operation = new();
            SampleChiSquaredOperationIn input = new()
            {
                Seed = seed,
                DegreesOfFreedom = degreesOfFreedom,
                Count = sampleCount
            };
            SampleChiSquaredOperationOut output = await operation.Perform(input);

            IEnumerable<double> samples = output.Samples;
            (double sampleMean, int _) = samples.ComputeRawMoment(1);
            Assert.AreEqual(degreesOfFreedom, sampleMean, 4 * Math.Sqrt((double)1 / sampleCount));
        }

        /// <summary>
        /// Tests that the random samples generated have approximately the expected deviation.
        /// </summary>
        /// <param name="sampleCount">The number of samples to generate.</param>
        /// <param name="seed">The seed used to seed the random operation.</param>
        /// <param name="degreesOfFreedom">The degrees of freedom parameter of the Chi-Squared distribution.</param>
        [TestCase(14500, 45612, 2)]
        [TestCase(100000, 45612, 2)]
        [TestCase(25000, 45612, 2)]
        [TestCase(25000, 45612, 5)]
        [TestCase(25000, 45612, 25)]
        public async Task TestDeviation(int sampleCount, int seed, int degreesOfFreedom)
        {
            SampleChiSquaredOperation operation = new();
            SampleChiSquaredOperationIn input = new()
            {
                Seed = seed,
                DegreesOfFreedom = degreesOfFreedom,
                Count = sampleCount
            };
            SampleChiSquaredOperationOut output = await operation.Perform(input);

            IEnumerable<double> samples = output.Samples;
            (double sampleVariance, int _) = samples.ComputeCentralMoment(2);
            double sampleDeviation = Math.Sqrt(sampleVariance);
            Assert.AreEqual(Math.Sqrt(2.0 * degreesOfFreedom), sampleDeviation, 4 * Math.Sqrt((double)4 / sampleCount));
        }

        /// <summary>
        /// Tests that the random samples generated have approximately the expected skewness.
        /// </summary>
        /// <param name="sampleCount">The number of samples to generate.</param>
        /// <param name="seed">The seed used to seed the random operation.</param>
        /// <param name="degreesOfFreedom">The degrees of freedom parameter of the Chi-Squared distribution.</param>
        [TestCase(14500, 45612, 3)]
        [TestCase(100000, 45612, 2)]
        [TestCase(25000, 45612, 2)]
        [TestCase(25000, 45612, 5)]
        [TestCase(25000, 45612, 25)]
        public async Task TestSkewness(int sampleCount, int seed, int degreesOfFreedom)
        {
            SampleChiSquaredOperation operation = new();
            SampleChiSquaredOperationIn input = new()
            {
                Seed = seed,
                DegreesOfFreedom = degreesOfFreedom,
                Count = sampleCount
            };
            SampleChiSquaredOperationOut output = await operation.Perform(input);

            IEnumerable<double> samples = output.Samples;
            (double sampleSkewness, int _) = samples.ComputeNormalizedMoment(3);
            Assert.AreEqual(Math.Sqrt(8.0 / degreesOfFreedom), sampleSkewness, 4 * Math.Sqrt((double)6 / sampleCount));
        }

        /// <summary>
        /// Tests that the random samples generated have approximately the expected kurtosis.
        /// </summary>
        /// <param name="sampleCount">The number of samples to generate.</param>
        /// <param name="seed">The seed used to seed the random operation.</param>
        /// <param name="degreesOfFreedom">The degrees of freedom parameter of the Chi-Squared distribution.</param>
        [TestCase(100000, 45613, 2)]
        [TestCase(25000, 45612, 2)]
        [TestCase(25000, 45612, 5)]
        [TestCase(25000, 45612, 25)]
        public async Task TestKurtosis(int sampleCount, int seed, int degreesOfFreedom)
        {
            SampleChiSquaredOperation operation = new();
            SampleChiSquaredOperationIn input = new()
            {
                Seed = seed,
                DegreesOfFreedom = degreesOfFreedom,
                Count = sampleCount
            };
            SampleChiSquaredOperationOut output = await operation.Perform(input);

            IEnumerable<double> samples = output.Samples;
            (double sampleKurtosis, int _) = samples.ComputeNormalizedMoment(4);
            Assert.AreEqual(3 + 12.0 / degreesOfFreedom, sampleKurtosis, 8 * Math.Sqrt((double)24 / sampleCount));
        }

        /// <summary>
        /// Tests that random samplees are same if they are generated with the same seed.
        /// </summary>
        /// <param name="seed">The seed to use.</param>
        [TestCase(0)]
        [TestCase(10)]
        [TestCase(42)]
        [TestCase(-1234)]
        [TestCase(int.MaxValue)]
        public async Task TestSeededness(int seed)
        {
            SampleChiSquaredOperation operation = new();
            SampleChiSquaredOperationIn input = new()
            {
                Seed = seed,
                DegreesOfFreedom = 2,
                Count = 1
            };
            SampleChiSquaredOperationOut output1 = await operation.Perform(input);
            SampleChiSquaredOperationOut output2 = await operation.Perform(input);

            Assert.AreEqual(output1.Samples, output2.Samples);
        }
    }
}