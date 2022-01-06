using System;
using System.Threading.Tasks;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="SamplePoissonOperation" /> operation.
    /// </summary>
    public struct SamplePoissonOperationIn
    {
        /// <summary>
        /// The seed for the psuedorandom number generator. If not specified, will be based on time.
        /// </summary>
        public int? Seed { get; set; }

        /// <summary>
        /// The rate (lambda) parameter of the Poisson distribution.
        /// </summary>
        public double Rate { get; set; }

        /// <summary>
        /// The number of samples to generate.
        /// </summary>
        public int Count { get; set; }
    }
    /// <summary>
    /// The input for the <see cref="SamplePoissonOperation" /> operation.
    /// </summary>
    public struct SamplePoissonOperationOut
    {
        /// <summary>
        /// The randomly generated samples picked from the Poisson distribution.
        /// </summary>
        public int[] Samples { get; set; }
    }

    /// <summary>
    /// Generates a sample of random numbers selected from a
    /// [Poisson distribution](https://en.wikipedia.org/wiki/Poisson_distribution).
    /// The random numbers are generated using an exponential simulation detailed in
    /// [this article](https://www.johndcook.com/blog/csharp_poisson/).
    /// </summary>
    [OperationName(Display = "Sample Poisson Distribution", Type = "samplePoisson")]
    [OperationTag(OperationTags.Statistics)]
    [OperationTag(OperationTags.Synthetic)]
    public class SamplePoissonOperation : TypedOperation
    <
        SamplePoissonOperationIn,
        SamplePoissonOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<SamplePoissonOperationOut> Perform(SamplePoissonOperationIn input)
        {
            // Create a PsuedoRNG with a seed if specified.
            Random random = input.Seed.HasValue
                ? new Random(input.Seed.Value)
                : new Random();

            // Generate the samples.
            int[] samples = new int[input.Count];
            for (int k = 0; k < samples.Length; k++)
            {
                // Generate a Poisson variable based on an exponential simulation.
                int occurrences = -1;
                double probability = 1.0;
                double lambdaExp = Math.Exp(-input.Rate);
                do
                {
                    // Generate a random uniform variable in (0, 1).
                    double uniform = random.NextDouble();

                    // Increment the occurrence (based on exponential distribution) and adjust probability.
                    occurrences++;
                    probability *= uniform;
                } while (probability > lambdaExp);

                samples[k] = occurrences;
            }

            // Return the samples.
            return Task.FromResult(new SamplePoissonOperationOut { Samples = samples });
        }

        /// <inheritdoc />
        public override bool IsDeterministic(SamplePoissonOperationIn input)
        {
            // This operation is deterministic only if seeded.
            return input.Seed.HasValue;
        }
    }
}