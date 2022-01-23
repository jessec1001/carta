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
        public double[] Values { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="KurtosisOperation" /> operation.
    /// </summary>
    public struct KurtosisOperationOut
    {
        /// <summary>
        /// The computed central kurtosis.
        /// </summary>
        public double Kurtosis { get; set; }
    }

    /// <summary>
    /// Calculates the central kurtosis of a set of numeric values.
    /// </summary>
    [OperationName(Display = "Statistical Kurtosis", Type = "statsKurtosis")]
    [OperationTag(OperationTags.Statistics)]
    public class KurtosisOperation : TypedOperation
    <
        KurtosisOperationIn,
        KurtosisOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<KurtosisOperationOut> Perform(KurtosisOperationIn input)
        {
            return Task.FromResult(new KurtosisOperationOut() { Kurtosis = input.Values.ComputeNormalizedMoment(4) });
        }
    }
}