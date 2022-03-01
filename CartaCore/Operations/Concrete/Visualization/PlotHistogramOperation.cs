using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations.Visualization
{
    /// <summary>
    /// A single datum for a bin in a histogram plot.
    /// </summary>
    public struct HistogramPlotBin
    {
        /// <summary>
        /// The frequency of a set of values in this bin.
        /// </summary>
        public double Frequency { get; set; }

        /// <summary>
        /// The minimum value in the range of this bin.
        /// </summary>
        public double Min { get; set; }
        /// <summary>
        /// The maximum value in the range of this bin.
        /// </summary>
        public double Max { get; set; }
    }
    /// <summary>
    /// The data for a histogram plot.
    /// </summary>
    public class HistogramPlot : Plot
    {
        /// <inheritdoc />
        public override string Type => "histogram";

        /// <summary>
        /// The bins for this histogram.
        /// </summary>
        public HistogramPlotBin[] Data { get; set; }

        /// <summary>
        /// The name of the colormap to use for mapping numeric data to colors. If not specified, a default color will
        /// be used instead.
        /// </summary>
        public string Colormap { get; init; }
    }

    /// <summary>
    /// The input for the <see cref="PlotHistogramOperation" /> operation.
    /// </summary>
    public struct PlotHistogramOperationIn
    {
        /// <summary>
        /// The title of the histogram visualization.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The list of values that make up the histogram.
        /// </summary>
        public double[] Values { get; set; }
        /// <summary>
        /// The number of bins to use for the histogram.
        /// </summary>
        public int Bins { get; set; }
        /// <summary>
        /// Whether to normalize the number of occurrences in each bin to the range [0, 1].
        /// </summary>
        public bool Normalize { get; set; }

        /// <summary>
        /// The value-axis for the histogram plot.
        /// </summary>
        public PlotAxis? ValueAxis { get; set; }
        /// <summary>
        /// The frequency-axis for the histogram plot.
        /// </summary>
        public PlotAxis? FrequencyAxis { get; set; }

        /// <summary>
        /// The name of the colormap to use for mapping numeric data to colors. If not specified, a default color will
        /// be used instead.
        /// </summary>
        public string ColorMap { get; set; }

        /// <summary>
        /// The style of the histogram bins if not assigned with greater specificity.
        /// </summary>
        public PlotStyle? BinStyle { get; set; }
        /// <summary>
        /// The style of the histogram plot axes.
        /// </summary>
        public PlotStyle? AxesStyle { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="PlotHistogramOperation" /> operation.
    /// </summary>
    public struct PlotHistogramOperationOut
    {
        /// <summary>
        /// The generated histogram plot.
        /// </summary>
        public HistogramPlot Plot { get; set; }
    }

    /// <summary>
    /// Visualizes a histogram plot of a set of values. 
    /// </summary>
    [OperationName(Display = "Histogram Plot", Type = "visualizeHistogramPlot")]
    [OperationTag(OperationTags.Visualization)]
    public class PlotHistogramOperation : TypedOperation
    <
        PlotHistogramOperationIn,
        PlotHistogramOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<PlotHistogramOperationOut> Perform(
            PlotHistogramOperationIn input,
            OperationJob job)
        {
            // Interpret the axes styles if specified.
            // Notice that we ignore the stroke width because it does not have meaning for the axes yet.
            Dictionary<string, string> styles = new();
            if (input.AxesStyle is not null)
            {
                PlotStyle axesStyle = input.AxesStyle.Value;
                if (axesStyle.FillColor is not null) styles["background"] = axesStyle.FillColor;
                if (axesStyle.StrokeColor is not null) styles["color"] = axesStyle.StrokeColor;
            }

            // Create the data for the histogram. This format should be compliant with the visualization library so that
            // it may be used directly from the front-end to generate visualizations.
            // This involves generating the frequencies inside each of the generated bins.
            double minimum = input.Values.Min();
            double maximum = input.Values.Max();
            double stepSize = (maximum - minimum) / input.Bins;

            // Generate each of the histogram plot bins.
            HistogramPlotBin[] bins = new HistogramPlotBin[input.Bins];
            for (int k = 0; k < input.Bins; k++)
            {
                double min = minimum + (k + 0) * stepSize;
                double max = minimum + (k + 1) * stepSize;
                double frequency = input.Values.Count(x => x >= min && x < max);
                bins[k] = new HistogramPlotBin
                {
                    Frequency = frequency,
                    Min = min,
                    Max = max
                };
            }

            // Generate the axes for the histogram plot.
            PlotAxis valueAxis = input.ValueAxis ?? new PlotAxis { Label = "Value" };
            PlotAxis frequencyAxis = input.FrequencyAxis ?? new PlotAxis { Label = "Frequency" };

            // Generate the data for the histogram.
            HistogramPlot plot = new()
            {
                Data = bins,
                Axes = new PlotAxes() { X = valueAxis, Y = frequencyAxis },
                Style = styles,
                Colormap = input.ColorMap
            };
            return Task.FromResult(new PlotHistogramOperationOut() { Plot = plot });
        }
    }
}