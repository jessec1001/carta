using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Extensions.Statistics;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="MeanOperation" /> operation.
    /// </summary>
    public struct MeanOperationIn
    {
        /// <summary>
        /// The list of numeric values to compute the mean of.
        /// </summary>
        [FieldName("Values")]
        public IAsyncEnumerable<double> Values { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="MeanOperation" /> operation.
    /// </summary>
    public struct MeanOperationOut
    {
        /// <summary>
        /// The computed mean.
        /// </summary>
        [FieldName("Mean")]
        public double Mean { get; set; }
    }

    /// <summary>
    /// Calculates the mean of a set of numeric values.
    /// </summary>
    [OperationName(Display = "Statistical Mean", Type = "statsMean")]
    [OperationTag(OperationTags.Statistics)]
    public class MeanOperation : TypedOperation
    <
        MeanOperationIn,
        MeanOperationOut
    >
    {
        /// <inheritdoc />
        public override async Task<MeanOperationOut> Perform(MeanOperationIn input)
        {
            // We calculate the mean asynchronously.
            (double mean, _) = await input.Values.ComputeRawMomentAsync(1);
            return new MeanOperationOut() { Mean = mean };
        }
    }
}