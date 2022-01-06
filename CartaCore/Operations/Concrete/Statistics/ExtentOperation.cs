using System.Threading.Tasks;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="ExtentOperation" /> operation.
    /// </summary>
    public struct ExtentOperationIn
    {
        /// <summary>
        /// The array of values to compute the extent of.
        /// </summary>
        public double[] Values { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="ExtentOperation" /> operation.
    /// </summary>
    public struct ExtentOperationOut
    {
        /// <summary>
        /// The minimum value of the array of values.
        /// </summary>
        public double Minimum { get; set; }
        /// <summary>
        /// The maximum value of the array of values.
        /// </summary>
        public double Maximum { get; set; }
    }

    /// <summary>
    /// Calculates the minimum and maximum values of a set of numeric values.
    /// </summary>
    [OperationName(Display = "Statistical Extent", Type = "statsExtent")]
    [OperationTag(OperationTags.Statistics)]
    public class ExtentOperation : TypedOperation
    <
        ExtentOperationIn,
        ExtentOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<ExtentOperationOut> Perform(ExtentOperationIn input)
        {
            // If there are no entries in the input array, the minimum and maximum are defined to be zero.
            double? minimum = null;
            double? maximum = null;
            foreach (double value in input.Values)
            {
                if (!minimum.HasValue || value < minimum) minimum = value;
                if (!maximum.HasValue || value > maximum) maximum = value;
            }
            return Task.FromResult
            (
                new ExtentOperationOut()
                {
                    Minimum = minimum.Value,
                    Maximum = maximum.Value
                }
            );
        }
    }
}