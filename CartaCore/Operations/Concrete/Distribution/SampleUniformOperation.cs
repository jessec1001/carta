using System;
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
        public int? Seed { get; set; }

        /// <summary>
        /// The minimum (inclusive) value for each sample.
        /// </summary>
        public double Minimum { get; set; }
        /// <summary>
        /// The maximum (exclusive) value for each sample.
        /// </summary>
        public double Maximum { get; set; }

        /// <summary>
        /// The number of samples to generate.
        /// </summary>
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
        public double[] Samples { get; set; }
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
        /// <inheritdoc />
        public override Task<SampleUniformOperationOut> Perform(SampleUniformOperationIn input)
        {
            // Create a PsuedoRNG with a seed if specified.
            Random random = input.Seed.HasValue
                ? new Random(input.Seed.Value)
                : new Random();

            // Generate the samples.
            double[] samples = new double[input.Count];
            for (int k = 0; k < samples.Length; k++)
            {
                // Generate a random uniform variable in [0, 1) and then scale it appropriately.
                double uniform = random.NextDouble();
                double sampleUniform = input.Minimum + (input.Maximum - input.Minimum) * uniform;

                samples[k] = sampleUniform;
            }

            // Return the samples.
            return Task.FromResult(new SampleUniformOperationOut { Samples = samples });
        }

        /// <inheritdoc />
        public override bool IsDeterministic(SampleUniformOperationIn input)
        {
            return input.Seed.HasValue;
        }
    }
}