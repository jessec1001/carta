using System.Threading.Tasks;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations.Visualization
{
    /// <summary>
    /// The output for the <see cref="PlotStyleOperation" /> operation.
    /// </summary>
    public struct PlotStyleOperationOut
    {
        /// <summary>
        /// The generated style.
        /// </summary>
        public PlotStyle Style { get; set; }
    }

    /// <summary>
    /// Creates a reusable style for a plot element.
    /// </summary>
    [OperationName(Display = "Plot Style", Type = "plotStyle")]
    [OperationTag(OperationTags.Visualization)]
    public class PlotStyleOperation : TypedOperation
    <
        PlotStyle,
        PlotStyleOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<PlotStyleOperationOut> Perform(PlotStyle input)
        {
            return Task.FromResult(new PlotStyleOperationOut() { Style = input });
        }
    }
}