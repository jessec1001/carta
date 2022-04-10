using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations.Distribution
{
    /// <summary>
    /// The input for the <see cref="SampleNormalOperation" /> operation.
    /// </summary>
    public struct SampleNormalOperationIn
    {
        /// <summary>
        /// The seed for the psuedorandom number generator. If not specified, will be based on time.
        /// </summary>
        [FieldName("Seed")]
        public int? Seed { get; set; }

        /// <summary>
        /// The mean of the normal distribution to sample from.
        /// </summary>
        [FieldDefault(0)]
        [FieldName("Mean")]
        public double Mean { get; set; }
        /// <summary>
        /// The standard deviation of the normal distribution to sample from.
        /// </summary>
        [FieldRange(Minimum = 0, ExclusiveMinimum = true)]
        [FieldDefault(1)]
        [FieldName("Deviation")]
        public double Deviation { get; set; }

        /// <summary>
        /// The number of samples to generate.
        /// </summary>
        [FieldRange(Minimum = 0, ExclusiveMinimum = true)]
        [FieldDefault(1)]
        [FieldName("Count")]
        public int Count { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="SampleNormalOperation" /> operation.
    /// </summary>
    public struct SampleNormalOperationOut
    {
        /// <summary>
        /// The randomly generated samples picked from the normal distribution.
        /// </summary>
        [FieldName("Samples")]
        public IEnumerable<double> Samples { get; set; }
    }

    /// <summary>
    /// Generates a sample of random numbers selected from a
    /// [Normal distribution](https://en.wikipedia.org/wiki/Normal_distribution).
    /// The random numbers are generated using the
    /// [Box-Muller transformation](https://en.wikipedia.org/wiki/Box%E2%80%93Muller_transform)
    /// </summary>
    [OperationName(Display = "Sample Normal Distribution", Type = "sampleNormal")]
    [OperationTag(OperationTags.Statistics)]
    [OperationTag(OperationTags.Synthetic)]
    [OperationHidden]
    public class SampleNormalOperation : TypedOperation
    <
        SampleNormalOperationIn,
        SampleNormalOperationOut
    >
    {
        private static IEnumerable<double> GenerateSamples(Random random, double mean, double deviation, int count)
        {
            for (int k = 0; k < count; k++)
            {
                // Generate two random uniform variables in (0, 1).
                double uniform1 = Math.Clamp(random.NextDouble(), double.MinValue, 1.0);
                double uniform2 = Math.Clamp(random.NextDouble(), double.MinValue, 1.0);

                // Generate the standard normally distributed sample.
                double standardNormal = Math.Sqrt(-2.0 * Math.Log(uniform1)) * Math.Cos(2.0 * Math.PI * uniform2);
                double sampleNormal = deviation * standardNormal + mean;

                yield return sampleNormal;
            }
        }

        /// <inheritdoc />
        public override Task<SampleNormalOperationOut> Perform(SampleNormalOperationIn input)
        {
            // Create a PsuedoRNG with a seed if specified.  
            Random random = input.Seed.HasValue
                ? new Random(input.Seed.Value)
                : new Random();

            // Generate the samples.
            return Task.FromResult(new SampleNormalOperationOut
            {
                Samples = GenerateSamples(random, input.Mean, input.Deviation, input.Count)
            });
        }

        /// <inheritdoc />
        public override Task<bool> IsDeterministic(SampleNormalOperationIn input)
        {
            // This operation is deterministic only if seeded.
            return Task.FromResult(input.Seed.HasValue);
        }
    }
}