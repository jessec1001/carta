using System.Threading.Tasks;
using CartaCore.Statistics;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="SkewOperation" /> operation.
    /// </summary>
    public struct SkewOperationIn
    {
        /// <summary>
        /// The list of numeric values to compute the central skewness of.
        /// </summary>
        public double[] Values { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="SkewOperation" /> operation.
    /// </summary>
    public struct SkewOperationOut
    {
        /// <summary>
        /// The computed central skewness.
        /// </summary>
        public double Skew { get; set; }
    }

    /// <summary>
    /// Calculates the central skewness of a set of numeric values.
    /// </summary>
    public class SkewOperation : TypedOperation
    <
        SkewOperationIn,
        SkewOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<SkewOperationOut> Perform(SkewOperationIn input)
        {
            return Task.FromResult(new SkewOperationOut()
                { Skew = StatisticsUtils.ComputeNormalizedMoment(input.Values, 3) }
            );
        }
    }
}