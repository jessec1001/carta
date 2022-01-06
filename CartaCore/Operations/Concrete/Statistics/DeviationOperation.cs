using System;
using System.Threading.Tasks;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="DeviationOperation" /> operation.
    /// </summary>
    public struct DeviationOperationIn
    {
        /// <summary>
        /// The array of values to compute the standard deviation of.
        /// </summary>
        public double[] Values { get; set; }
        /// <summary>
        /// Whether to use [Bessel's Correction](https://en.wikipedia.org/wiki/Bessel%27s_correction) to make the
        /// estimator of deviation unbiased.
        /// </summary>
        public bool UseBesselsCorrection { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="DeviationOperation" /> operation.
    /// </summary>
    public struct DeviationOperationOut
    {
        /// <summary>
        /// The standard deviation of the list of values. 
        /// </summary>
        public double Deviation { get; set; }
    }

    /// <summary>
    /// Calculates the standard deviation of a set of numeric values.
    /// </summary>
    [OperationName(Display = "Statistical Deviation", Type = "statsDeviation")]
    [OperationTag(OperationTags.Statistics)]
    public class DeviationOperation : TypedOperation
    <
        DeviationOperationIn,
        DeviationOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<DeviationOperationOut> Perform(DeviationOperationIn input)
        {
            // We use these values to accumulate statistics of the input values.
            double sum = 0.0;
            double sumOfSquares = 0.0;
            int count = input.Values.Length;

            // We perform the accumulation of sums.
            foreach (double value in input.Values)
            {
                sum += value;
                sumOfSquares += value * value;
            }

            // We compute the variance using the algebraic equivalent of "V = E[X^2] - E[X]^2".
            // Note that if Bessel's correction should be used, we multiply by a factor of n/(n-1).
            double variance = (sumOfSquares / count) - Math.Pow(sum / count, 2);
            if (input.UseBesselsCorrection)
            {
                // We cannot compute Bessel's correction with fewer than two values.
                variance *= (double)count / (count - 1);
            }
            double deviation = Math.Sqrt(variance);

            // Return the output.
            return Task.FromResult(new DeviationOperationOut { Deviation = deviation });
        }
    }
}