using System.Threading.Tasks;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations.Visualization
{
    /// <summary>
    /// The output for the <see cref="PlotAxisOperation" /> operation.
    /// </summary>
    public struct PlotAxisOperationOut
    {
        /// <summary>
        /// The generated axis.
        /// </summary>
        public PlotAxis Axis { get; set; }
    }

    /// <summary>
    /// Creates a reusable axis for a plot.
    /// </summary>
    [OperationName(Display = "Plot Axis", Type = "plotAxis")]
    [OperationTag(OperationTags.Visualization)]
    public class PlotAxisOperation : TypedOperation
    <
        PlotAxis,
        PlotAxisOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<PlotAxisOperationOut> Perform(PlotAxis input)
        {
            return Task.FromResult(new PlotAxisOperationOut() { Axis = input });
        }
    }
}