using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations.Visualization
{
    /// <summary>
    /// A single datum in a scatter plot.
    /// </summary>
    public struct ScatterPlotDatum
    {
        /// <summary>
        /// The x-position of this scatter plot point.
        /// </summary>
        public double? X { get; set; }
        /// <summary>
        /// The y-position of this scatter plot point.
        /// </summary>
        public double? Y { get; set; }
        /// <summary>
        /// The z-position of this scatter plot point.
        /// </summary>
        public double? Z { get; set; }

        /// <summary>
        /// The radius of this scatter plot point.
        /// </summary>
        public double? Radius { get; set; }

        // TODO: Figure out how to temporarily allow for color and colormap value until colormaps are handled backend.
        /// <summary>
        /// The value of the scatter plot point. Used for coloring if a colormap is specified.
        /// </summary>
        public double? Value { get; set; }
        /// <summary>
        /// The style of the scatter plot point.
        /// </summary>
        public PlotStyle? Style { get; set; }
    }
    /// <summary>
    /// The base structure of the scatter plot.
    /// </summary>
    public class ScatterPlotBase : Plot
    {
        /// <inheritdoc />
        public override string Type => "scatter";

        /// <summary>
        /// The name of the colormap to use for mapping numeric data to colors. If not specified, a default color will
        /// be used instead.
        /// </summary>
        public string Colormap { get; init; }
    }
    /// <summary>
    /// The data for a scatter plot.
    /// </summary>
    public class ScatterPlot : ScatterPlotBase, IAsyncPipelineable<ScatterPlot, ScatterPlotBase, ScatterPlotDatum>
    {
        /// <summary>
        /// The data points for this scatter plot.
        /// </summary>
        public IAsyncEnumerable<ScatterPlotDatum> Data { get; set; }

        /// <inheritdoc />
        public ScatterPlotBase Deconstruct()
        {
            return new ScatterPlotBase()
            {
                Colormap = Colormap,
            };
        }
        /// <inheritdoc />
        public IAsyncEnumerable<ScatterPlotDatum> Enumerate()
        {
            return Data;
        }
        /// <inheritdoc />
        public Task<ScatterPlot> Renumerate(IAsyncEnumerable<ScatterPlotDatum> elements, ScatterPlotBase structure)
        {
            ScatterPlot plot = new ScatterPlot()
            {
                Colormap = structure.Colormap,
                Data = elements,
            };
            return Task.FromResult(plot);
        }
    }

    /// <summary>
    /// The input for the <see cref="ScatterPlotOperation" /> operation.
    /// </summary>
    public struct ScatterPlotOperationIn
    {
        /// <summary>
        /// The title of the scatter plot visualization.
        /// </summary>
        [FieldName("Title")]
        public string Title { get; set; }

        /// <summary>
        /// The x-values for the points in the scatter plot. 
        /// </summary>
        [FieldName("x-Axis Values")]
        public IAsyncEnumerable<double?> XValues { get; set; }
        /// <summary>
        /// The y-values for the points in the scatter plot.
        /// </summary>
        [FieldName("y-Axis Values")]
        public IAsyncEnumerable<double?> YValues { get; set; }
        /// <summary>
        /// The z-values for the points in the scatter plot.
        /// If not specified, will be a 2D scatter plot.
        /// </summary>
        [FieldName("z-Axis Values")]
        public IAsyncEnumerable<double?> ZValues { get; set; }
        /// <summary>
        /// The x-axis for the scatter plot.
        /// </summary>
        [FieldName("x-Axis")]
        public PlotAxis? XAxis { get; set; }
        /// <summary>
        /// The y-axis for the scatter plot.
        /// </summary>
        [FieldName("y-Axis")]
        public PlotAxis? YAxis { get; set; }
        /// <summary>
        /// The z-axis for the scatter plot.
        /// </summary>
        [FieldName("z-Axis")]
        public PlotAxis? ZAxis { get; set; }

        // TODO: Allow for specifying an option for zoom-independent radii instead of being default.
        /// <summary>
        /// The radii of the points in the scatter plot.
        /// This field is optional.
        /// </summary>
        [FieldName("Radii")]
        public IAsyncEnumerable<double?> Radius { get; set; }

        // TODO: Make an enumeration of all possible colormaps available in the visualization library.
        /// <summary>
        /// The color map to use for the scatter plot. If specified, the color values indicate the color of the point.
        /// </summary>
        [FieldName("Color Map")]
        public string ColorMap { get; set; }
        /// <summary>
        /// The values to use to color the points in the scatterplot when applying the colormap.
        /// </summary>
        [FieldName("Color Values")]
        public IAsyncEnumerable<double?> ColorValues { get; set; }

        /// <summary>
        /// The style of the scatter plot points if not assigned with greater specificity.
        /// </summary>
        [FieldName("Point Style")]
        public PlotStyle? PointStyle { get; set; }
        /// <summary>
        /// The style of the scatter plot axes.
        /// </summary>
        [FieldName("Axes Style")]
        public PlotStyle? AxesStyle { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="ScatterPlotOperation" /> operation.
    /// </summary>
    public struct ScatterPlotOperationOut
    {
        /// <summary>
        /// The generated scatter plot.
        /// </summary>
        [FieldName("Plot")]
        public ScatterPlot Plot { get; set; }
    }


    /// <summary>
    /// Visualizes a scatter plot of 2D or 3D data.
    /// </summary>
    [OperationName(Display = "Scatter Plot", Type = "visualizeScatterPlot")]
    [OperationTag(OperationTags.Visualization)]
    public class ScatterPlotOperation : TypedOperation
    <
        ScatterPlotOperationIn,
        ScatterPlotOperationOut
    >
    {
        private static async IAsyncEnumerable<ScatterPlotDatum> EnumerateData(
            IAsyncEnumerator<double?> xValues,
            IAsyncEnumerator<double?> yValues,
            IAsyncEnumerator<double?> zValues,
            IAsyncEnumerator<double?> radii,
            IAsyncEnumerator<double?> colors,
            string colormap,
            PlotStyle? style
        )
        {
            // TODO: In the future, we should calculate colors from colormaps here rather than in the visualization library.
            // Generate each of the scatter plot points.
            while (true)
            {
                bool moreElements = true;
                moreElements &= xValues is null || await xValues.MoveNextAsync();
                moreElements &= yValues is null || await yValues.MoveNextAsync();
                moreElements &= zValues is null || await zValues.MoveNextAsync();
                moreElements &= radii is null || await radii.MoveNextAsync();
                moreElements &= colors is null || await colors.MoveNextAsync();
                if (!moreElements) break;

                double? x = xValues?.Current;
                double? y = yValues?.Current;
                double? z = zValues?.Current;
                double? r = radii?.Current;
                double? c = colors?.Current;

                yield return new ScatterPlotDatum
                {
                    X = x,
                    Y = y,
                    Z = z,
                    Radius = r,
                    Value = colormap is null ? null : c,
                    Style = style,
                };
            }
        }

        /// <inheritdoc />
        public override Task<ScatterPlotOperationOut> Perform(
            ScatterPlotOperationIn input,
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

            // We assume that the dimensions are specified in order of x, y, z and that there are at least 2 dimensions.
            IAsyncEnumerable<double?> X = input.XValues;
            IAsyncEnumerable<double?> Y = input.YValues;
            IAsyncEnumerable<double?> Z = input.ZValues;
            IAsyncEnumerable<double?> R = input.Radius;
            IAsyncEnumerable<double?> C = input.ColorValues;

            // First, we check that there at least 2 dimensions.
            if (X is null || Y is null)
                throw new ArgumentException("Scatter plot must have at least 2 dimensions.");

            // Third, we check that color values are specified if a colormap is specified.
            if (C is null && input.ColorMap is not null)
                throw new ArgumentException("Scatter plot color values must be specified when a color map is specified.");

            // Generate the axes for the scatter plot when not specified.
            PlotAxis xAxis = input.XAxis ?? new PlotAxis() { Label = "X" };
            PlotAxis yAxis = input.YAxis ?? new PlotAxis() { Label = "Y" };
            PlotAxis zAxis = input.ZAxis ?? new PlotAxis() { Label = "Z" };

            // Create a new scatter plot structure.
            // Based on what the input data looks like, we'll generate a 2D or 3D scatter plot.
            ScatterPlot plot = new()
            {
                Data = EnumerateData(
                    X?.GetAsyncEnumerator(),
                    Y?.GetAsyncEnumerator(),
                    Z?.GetAsyncEnumerator(),
                    R?.GetAsyncEnumerator(),
                    C?.GetAsyncEnumerator(),
                    input.ColorMap,
                    input.PointStyle),
                Axes = new PlotAxes() { X = xAxis, Y = yAxis, Z = zAxis },
                Style = styles,
                Colormap = input.ColorMap,
            };
            return Task.FromResult(new ScatterPlotOperationOut() { Plot = plot });
        }
    }
}