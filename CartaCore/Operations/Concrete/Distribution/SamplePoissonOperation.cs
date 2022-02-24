using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations.Distribution
{
    /// <summary>
    /// The input for the <see cref="SamplePoissonOperation" /> operation.
    /// </summary>
    public struct SamplePoissonOperationIn
    {
        /// <summary>
        /// The seed for the psuedorandom number generator. If not specified, will be based on time.
        /// </summary>
        [FieldName("Seed")]
        public int? Seed { get; set; }

        /// <summary>
        /// The rate (lambda) parameter of the Poisson distribution.
        /// </summary>
        [FieldRange(Minimum = 0, ExclusiveMinimum = true)]
        [FieldDefault(1)]
        [FieldName("Rate")]
        public double Rate { get; set; }

        /// <summary>
        /// The number of samples to generate.
        /// </summary>
        [FieldRange(Minimum = 0, ExclusiveMinimum = true)]
        [FieldDefault(1)]
        [FieldName("Count")]
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
        [FieldName("Samples")]
        public IEnumerable<int> Samples { get; set; }
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
        private static IEnumerable<int> GenerateSamples(Random random, double rate, int count)
        {
            for (int k = 0; k < count; k++)
            {
                // Generate a Poisson variable based on an exponential simulation.
                int occurrences = -1;
                double probability = 1.0;
                double lambdaExp = Math.Exp(-rate);
                do
                {
                    // Generate a random uniform variable in (0, 1).
                    double uniform = random.NextDouble();

                    // Increment the occurrence (based on exponential distribution) and adjust probability.
                    occurrences++;
                    probability *= uniform;
                } while (probability > lambdaExp);

                yield return occurrences;
            }
        }

        /// <inheritdoc />
        public override Task<SamplePoissonOperationOut> Perform(SamplePoissonOperationIn input)
        {
            // Create a PsuedoRNG with a seed if specified.
            Random random = input.Seed.HasValue
                ? new Random(input.Seed.Value)
                : new Random();

            // Generate the samples.
            return Task.FromResult(new SamplePoissonOperationOut
            {
                Samples = GenerateSamples(random, input.Rate, input.Count)
            });
        }

        /// <inheritdoc />
        public override bool IsDeterministic(SamplePoissonOperationIn input)
        {
            // This operation is deterministic only if seeded.
            return input.Seed.HasValue;
        }
    }
}