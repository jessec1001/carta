using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using CartaCore.Operations.Attributes;
using CartaCore.Operations.Visualization;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="PlotHistogramOperation" /> operation.
    /// </summary>
    public struct PlotHistogramOperationIn
    {
        /// <summary>
        /// The name of the output visualization.
        /// </summary>
        public string Name { get; set; }

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
        /// The color to apply to the entire histogram.
        /// Must be one of the following:
        /// - A common color name such as "blue" or "tomato".
        /// - A hex encoded color in the form "#1267bc".
        /// </summary>
        public string Color { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="PlotHistogramOperation" /> operation.
    /// </summary>
    public struct PlotHistogramOperationOut { }

    /// <summary>
    /// Visualizes a histogram plot of a set of values. 
    /// </summary>
    [OperationName(Display = "Histogram Plot", Type = "visualizeHistogramPlot")]
    [OperationTag(OperationTags.Visualization)]
    [OperationVisualizer("histogram")]
    public class PlotHistogramOperation : TypedOperation
    <
        PlotHistogramOperationIn,
        PlotHistogramOperationOut
    >
    {
        /// <summary>
        /// The value of a histogram plot bin including frequency and range spanned.
        /// </summary>
        private struct HistogramPlotValue
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

        /// <inheritdoc />
        public override Task<PlotHistogramOperationOut> Perform(
            PlotHistogramOperationIn input,
            OperationContext callingContext)
        {
            // Grab the color (if specified) as a hex code that can be passed to the visualization library.
            Color color = Color.FromArgb(83, 184, 83); // Carta theme color: #53b853
            try
            {
                if (input.Color is not null)
                    color = ColorTranslator.FromHtml(input.Color);
            }
            catch { }

            // Create the data for the histogram. This format should be compliant with the visualization library so that
            // it may be used directly from the front-end to generate visualizations.
            // This involves generating the frequencies inside each of the generated bins.
            double minimum = input.Values.Min();
            double maximum = input.Values.Max();
            double stepSize = (maximum - minimum) / input.Bins;

            // Generate the data for the histogram.
            PlotData<HistogramPlotValue> histogramPlot = new()
            {
                Data = Enumerable
                    .Range(0, input.Bins)
                    .Select((index) =>
                        {
                            double minimumBin = minimum + stepSize * (index + 0);
                            double maximumBin = minimum + stepSize * (index + 1);
                            int frequency = input.Values
                                .Where(value => value >= minimumBin && value < maximumBin)
                                .Count();
                            return new HistogramPlotValue()
                            {
                                Frequency = input.Normalize
                                    ? frequency / (double)input.Values.Length
                                    : frequency,
                                Min = minimumBin,
                                Max = maximumBin
                            };
                        }
                    )
                    .ToArray(),
                Axes = new PlotAxes()
                {
                    X = new PlotAxis() { Label = "Values" },
                    Y = new PlotAxis() { Label = input.Normalize ? "Frequencies" : "Occurrences" }
                },
                Color = ColorTranslator.ToHtml(color)
            };

            // Output the visualization data to the calling context.
            if (callingContext is not null && callingContext.Output.TryAdd(input.Name, histogramPlot))
                return Task.FromResult(new PlotHistogramOperationOut());
            else
                throw new ArgumentException($"Cannot set visualization '{input.Name}'.");
        }
    }
}