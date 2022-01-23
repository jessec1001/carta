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
        public double[] Values { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="MeanOperation" /> operation.
    /// </summary>
    public struct MeanOperationOut
    {
        /// <summary>
        /// The computed mean.
        /// </summary>
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
        public override Task<MeanOperationOut> Perform(MeanOperationIn input)
        {
            return Task.FromResult(new MeanOperationOut() { Mean = input.Values.ComputeMean() });
        }
    }
}