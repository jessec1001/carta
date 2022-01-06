using System.Threading.Tasks;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="SampleChiSquaredOperation" /> operation.
    /// </summary>
    public struct SampleChiSquaredOperationIn
    {
        /// <summary>
        /// The seed for the psuedorandom number generator. If not specified, will be based on time.
        /// </summary>
        public int? Seed { get; set; }

        /// <summary>
        /// The number of degrees of freedom of the Chi-Squared distribution.
        /// </summary>
        public int DegreesOfFreedom { get; set; }

        /// <summary>
        /// The number of samples to generate.
        /// </summary>
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
        public double[] Samples { get; set; }
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
        ///  <inheritdoc />
        public override async Task<SampleChiSquaredOperationOut> Perform(SampleChiSquaredOperationIn input)
        {
            // We utilize the normal distribution to generate standard normal samples.
            // Then, we apply Cochran's theorem to generate the desired Chi-Squared samples.
            SampleNormalOperation sampleNormalOperation = new();
            SampleNormalOperationIn sampleNormalOperationIn = new()
            {
                Seed = input.Seed,
                Count = input.Count * (input.DegreesOfFreedom + 1)
            };
            SampleNormalOperationOut sampleNormalOperationOut = await sampleNormalOperation.Perform(sampleNormalOperationIn);

            // For each set of (df + 1) standard normal samples, we generate a Chi-Squared sample.
            // The Chi-Squared sample is the sum of the squares of the (df + 1) standard normal samples.
            double[] samples = new double[input.Count];
            for (int k = 0; k < input.Count; k++)
            {
                double sum = 0;
                for (int j = 0; j < input.DegreesOfFreedom + 1; j++)
                {
                    double sample = sampleNormalOperationOut.Samples[k * (input.DegreesOfFreedom + 1) + j];
                    sum += sample * sample;
                }
                samples[k] = sum;
            }

            return new SampleChiSquaredOperationOut { Samples = samples };
        }
    }
}