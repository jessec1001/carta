using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Extensions.Statistics;
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
        public IAsyncEnumerable<double> Values { get; set; }
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
        public override async Task<DeviationOperationOut> Perform(DeviationOperationIn input)
        {
            // We calculate the deviation asynchronously.
            (double variance, int count) = await input.Values.ComputeCentralMomentAsync(2);
            if (input.UseBesselsCorrection)
                variance *= (double)count / (count - 1);
            double deviation = Math.Sqrt(variance);

            // Return the output.
            return new DeviationOperationOut { Deviation = deviation };
        }
    }
}