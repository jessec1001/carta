using System.Threading.Tasks;
using CartaCore.Statistics;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="MedianOperation"/> operation.
    /// </summary>
    public struct MedianOperationIn
    {
        /// <summary>
        /// The list of numeric values to compute the median of.
        /// </summary>
        public double[] Values { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="MedianOperation"/> operation.
    /// </summary>
    public struct MedianOperationOut
    {
        /// <summary>
        /// The computed median.
        /// </summary>
        public double Median { get; set; }
    }

    /// <summary>
    /// Calculates the median of a set of numeric values.
    /// </summary>
    public class MedianOperation : TypedOperation
    <
        MedianOperationIn,
        MedianOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<MedianOperationOut> Perform(MedianOperationIn input)
        {
            return Task.FromResult(
                new MedianOperationOut() { Median = StatisticsUtils.ComputeMedian(input.Values) }
            );
        }
    }
}