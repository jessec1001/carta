using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Extensions.Statistics;
using CartaCore.Operations.Attributes;

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
        [FieldName("Values")]
        public IAsyncEnumerable<double> Values { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="SkewOperation" /> operation.
    /// </summary>
    public struct SkewOperationOut
    {
        /// <summary>
        /// The computed central skewness.
        /// </summary>
        [FieldName("Skew")]
        public double Skew { get; set; }
    }

    /// <summary>
    /// Calculates the central skewness of a set of numeric values.
    /// </summary>
    [OperationName(Display = "Statistical Skew", Type = "statsSkew")]
    [OperationTag(OperationTags.Statistics)]
    public class SkewOperation : TypedOperation
    <
        SkewOperationIn,
        SkewOperationOut
    >
    {
        /// <inheritdoc />
        public override async Task<SkewOperationOut> Perform(SkewOperationIn input)
        {
            // We calculate the skewness asynchronously.
            (double skew, _) = await input.Values.ComputeNormalizedMomentAsync(3);
            return new SkewOperationOut() { Skew = skew };
        }
    }
}