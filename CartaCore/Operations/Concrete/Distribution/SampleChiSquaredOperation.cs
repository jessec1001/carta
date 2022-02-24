using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations.Distribution
{
    /// <summary>
    /// The input for the <see cref="SampleChiSquaredOperation" /> operation.
    /// </summary>
    public struct SampleChiSquaredOperationIn
    {
        /// <summary>
        /// The seed for the psuedorandom number generator. If not specified, will be based on time.
        /// </summary>
        [FieldName("Seed")]
        public int? Seed { get; set; }

        /// <summary>
        /// The number of degrees of freedom of the Chi-Squared distribution.
        /// </summary>
        [FieldRange(Minimum = 0, ExclusiveMinimum = true)]
        [FieldDefault(1)]
        [FieldName("Degrees of Freedom")]
        public int DegreesOfFreedom { get; set; }

        /// <summary>
        /// The number of samples to generate.
        /// </summary>
        [FieldRange(Minimum = 0, ExclusiveMinimum = true)]
        [FieldDefault(1)]
        [FieldName("Count")]
        public int Count { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="SampleChiSquaredOperation" /> operation.
    /// </summary>
    public struct SampleChiSquaredOperationOut
    {
        /// <summary>
        /// The randomly generated samples picked from the Chi-Squared distribution.
        /// </summary>
        [FieldName("Samples")]
        public IEnumerable<double> Samples { get; set; }
    }

    /// <summary>
    /// Generates a sample of numbers selected from a
    /// [Chi-Squared distribution](https://en.wikipedia.org/wiki/Chi-squared_distribution).
    /// Utilizes Cochran's theorem to calculate Chi-Squared samples from normal samples.
    /// </summary>
    [OperationName(Display = "Sample Chi-Squared Distribution", Type = "sampleChiSquared")]
    [OperationTag(OperationTags.Statistics)]
    [OperationTag(OperationTags.Synthetic)]
    public class SampleChiSquaredOperation : TypedOperation
    <
        SampleChiSquaredOperationIn,
        SampleChiSquaredOperationOut
    >
    {
        private static IEnumerable<double> GenerateSamples(IEnumerator<double> samples, int df, int count)
        {
            // Then, we apply Cochran's theorem to generate the desired Chi-Squared samples.
            for (int k = 0; k < count; k++)
            {
                // For each set of (df + 1) standard normal samples, we generate a Chi-Squared sample.
                // The Chi-Squared sample is the sum of the squares of the (df + 1) standard normal samples.
                double sum = 0;
                for (int j = 0; j < df; j++)
                {
                    samples.MoveNext();
                    double sample = samples.Current;
                    sum += sample * sample;
                }
                yield return sum;
            }
        }

        ///  <inheritdoc />
        public override async Task<SampleChiSquaredOperationOut> Perform(SampleChiSquaredOperationIn input)
        {
            // We utilize the normal distribution to generate standard normal samples.
            SampleNormalOperation sampler = new();
            SampleNormalOperationIn samplerIn = new()
            {
                Seed = input.Seed,
                Count = input.Count * input.DegreesOfFreedom,
                Mean = 0,
                Deviation = 1
            };
            SampleNormalOperationOut samplerOut = await sampler.Perform(samplerIn);

            // Generate the samples.
            IEnumerator<double> samples = samplerOut.Samples.GetEnumerator();
            return new() { Samples = GenerateSamples(samples, input.DegreesOfFreedom, input.Count) };
        }
    }
}