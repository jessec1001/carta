using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations.Distribution
{
    /// <summary>
    /// The input for the <see cref="SampleUniformOperation" /> operation.
    /// </summary>
    public struct SampleUniformOperationIn
    {
        /// <summary>
        /// The seed for the psuedorandom number generator. If not specified, will be based on time.
        /// </summary>
        [FieldName("Seed")]
        public int? Seed { get; set; }

        /// <summary>
        /// The minimum (inclusive) value for each sample.
        /// </summary>
        [FieldDefault(0)]
        [FieldName("Minimum")]
        public double Minimum { get; set; }
        /// <summary>
        /// The maximum (exclusive) value for each sample.
        /// </summary>
        [FieldDefault(1)]
        [FieldName("Maximum")]
        public double Maximum { get; set; }

        /// <summary>
        /// The number of samples to generate.
        /// </summary>
        [FieldRange(Minimum = 0, ExclusiveMinimum = true)]
        [FieldDefault(1)]
        [FieldName("Count")]
        public int Count { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="SampleUniformOperation" /> operation.
    /// </summary>
    public struct SampleUniformOperationOut
    {
        /// <summary>
        /// The randomly generated samples picked from the uniform distribution.
        /// </summary>
        [FieldName("Samples")]
        public IEnumerable<double> Samples { get; set; }
    }

    /// <summary>
    /// Generates a sample of numbers selected from a
    /// [continuous uniform distribution](https://en.wikipedia.org/wiki/Continuous_uniform_distribution).
    /// </summary>
    [OperationName(Display = "Sample Uniform Distribution", Type = "sampleUniform")]
    [OperationTag(OperationTags.Statistics)]
    [OperationTag(OperationTags.Synthetic)]
    public class SampleUniformOperation : TypedOperation
    <
        SampleUniformOperationIn,
        SampleUniformOperationOut
    >
    {
        private static IEnumerable<double> GenerateSamples(Random random, double min, double max, int count)
        {
            for (var k = 0; k < count; k++)
                yield return random.NextDouble() * (max - min) + min;
        }

        /// <inheritdoc />
        public override Task<SampleUniformOperationOut> Perform(SampleUniformOperationIn input)
        {
            // Create a PsuedoRNG with a seed if specified.
            Random random = input.Seed.HasValue
                ? new Random(input.Seed.Value)
                : new Random();

            // Generate the samples.
            return Task.FromResult(new SampleUniformOperationOut
            {
                Samples = GenerateSamples(random, input.Minimum, input.Maximum, input.Count)
            });
        }

        /// <inheritdoc />
        public override Task<bool> IsDeterministic(SampleUniformOperationIn input)
        {
            // This operation is deterministic only if seeded.
            return Task.FromResult(input.Seed.HasValue);
        }
    }
}