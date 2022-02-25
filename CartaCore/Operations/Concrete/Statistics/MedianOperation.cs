using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Extensions.Statistics;
using CartaCore.Operations.Attributes;

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
        [FieldName("Values")]
        public IAsyncEnumerable<double> Values { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="MedianOperation"/> operation.
    /// </summary>
    public struct MedianOperationOut
    {
        /// <summary>
        /// The computed median.
        /// </summary>
        [FieldName("Median")]
        public double Median { get; set; }
    }

    /// <summary>
    /// Calculates the median of a set of numeric values.
    /// </summary>
    [OperationName(Display = "Statistical Median", Type = "statsMedian")]
    [OperationTag(OperationTags.Statistics)]
    public class MedianOperation : TypedOperation
    <
        MedianOperationIn,
        MedianOperationOut
    >
    {
        /// <inheritdoc />
        public override async Task<MedianOperationOut> Perform(MedianOperationIn input)
        {
            // We calculate the median asynchronously.
            // Note that we do not actually have an algorithm to do this efficiently.
            double median = await input.Values.ComputeMedianAsync(); 
            return new MedianOperationOut() { Median = median };
        }
    }
}