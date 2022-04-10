using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Extensions.Statistics;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="KurtosisOperation" /> operation.
    /// </summary>
    public struct KurtosisOperationIn
    {
        /// <summary>
        /// The list of numeric values to compute the central kurtosis of.
        /// </summary>
        [FieldName("Values")]
        public IAsyncEnumerable<double> Values { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="KurtosisOperation" /> operation.
    /// </summary>
    public struct KurtosisOperationOut
    {
        /// <summary>
        /// The computed central kurtosis.
        /// </summary>
        [FieldName("Kurtosis")]
        public double Kurtosis { get; set; }
    }

    /// <summary>
    /// Calculates the central kurtosis of a set of numeric values.
    /// </summary>
    [OperationName(Display = "Statistical Kurtosis", Type = "statsKurtosis")]
    [OperationTag(OperationTags.Statistics)]
    [OperationHidden]
    public class KurtosisOperation : TypedOperation
    <
        KurtosisOperationIn,
        KurtosisOperationOut
    >
    {
        /// <inheritdoc />
        public override async Task<KurtosisOperationOut> Perform(KurtosisOperationIn input)
        {
            // We calculate the kurtosis asynchronously.
            (double kurtosis, _) = await input.Values.ComputeNormalizedMomentAsync(4);
            return new KurtosisOperationOut() { Kurtosis = kurtosis };
        }
    }
}