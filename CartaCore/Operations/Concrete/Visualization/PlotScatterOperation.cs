using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Operations.Attributes;

// TODO Move operations to their appropriate subnamespace.
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
    /// The data for a scatter plot.
    /// </summary>
    public class ScatterPlot : Plot
    {
        /// <inheritdoc />
        public override string Type => "scatter";

        /// <summary>
        /// The data points for this scatter plot.
        /// </summary>
        public ScatterPlotDatum[] Data { get; set; }

        /// <summary>
        /// The name of the colormap to use for mapping numeric data to colors. If not specified, a default color will
        /// be used instead.
        /// </summary>
        public string Colormap { get; init; }
    }

    /// <summary>
    /// The input for the <see cref="ScatterPlotOperation" /> operation.
    /// </summary>
    public struct ScatterPlotOperationIn
    {
        /// <summary>
        /// The title of the scatter plot visualization.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The x-values for the points in the scatter plot. 
        /// </summary>
        public double[] XValues { get; set; }
        /// <summary>
        /// The y-values for the points in the scatter plot.
        /// </summary>
        public double[] YValues { get; set; }
        /// <summary>
        /// The z-values for the points in the scatter plot.
        /// If not specified, will be a 2D scatter plot.
        /// </summary>
        public double[] ZValues { get; set; }
        /// <summary>
        /// The x-axis for the scatter plot.
        /// </summary>
        public PlotAxis? XAxis { get; set; }
        /// <summary>
        /// The y-axis for the scatter plot.
        /// </summary>
        public PlotAxis? YAxis { get; set; }
        /// <summary>
        /// The z-axis for the scatter plot.
        /// </summary>
        public PlotAxis? ZAxis { get; set; }

        // TODO: Allow for specifying an option for zoom-independent radii instead of being default.
        /// <summary>
        /// The radii of the points in the scatter plot.
        /// </summary>
        public double[] Radius { get; set; }

        // TODO: Make an enumeration of all possible colormaps available in the visualization library.
        /// <summary>
        /// The color map to use for the scatter plot. If specified, the color values indicate the color of the point.
        /// </summary>
        public string ColorMap { get; set; }
        /// <summary>
        /// The values to use to color the points in the scatterplot when applying the colormap.
        /// </summary>
        public double[] ColorValues { get; set; }

        /// <summary>
        /// The style of the scatter plot points if not assigned with greater specificity.
        /// </summary>
        public PlotStyle? PointStyle { get; set; }
        /// <summary>
        /// The style of the scatter plot axes.
        /// </summary>
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
        /// <inheritdoc />
        public override Task<ScatterPlotOperationOut> Perform(
            ScatterPlotOperationIn input,
            OperationContext callingContext)
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
            double[] x = input.XValues;
            double[] y = input.YValues;
            double[] z = input.ZValues;
            double[] r = input.Radius;
            double[] c = input.ColorValues;
            int length;

            // First, we check that there at least 2 dimensions.
            if (x is null || y is null)
                throw new ArgumentException("Scatter plot must have at least 2 dimensions.");
            length = x.Length;

            // Second, we check that the data in each axis is the same length.
            if (y.Length != length)
                throw new ArgumentException("Scatter plot values in x and y dimensions must be the same length.");
            if (z is not null && z.Length != length)
                throw new ArgumentException("Scatter plot values in x and z dimensions must be the same length.");
            if (r is not null && r.Length != length)
                throw new ArgumentException("Scatter plot values and radii must be the same length.");
            if (c is not null && c.Length != length && input.ColorMap is not null)
                throw new ArgumentException("Scatter plot values and color values must be the same length.");

            // Third, we check that color values are specified if a colormap is specified.
            if (c is null && input.ColorMap is not null)
                throw new ArgumentException("Scatter plot color values must be specified when a color map is specified.");

            // TODO: In the future, we should calculate colors from colormaps here rather than in the visualization library.
            // Generate each of the scatter plot points.
            ScatterPlotDatum[] points = new ScatterPlotDatum[length];
            for (int k = 0; k < length; k++)
            {
                ScatterPlotDatum point = new()
                {
                    X = x?[k],
                    Y = y?[k],
                    Z = z?[k],
                    Radius = r?[k],
                    Style = input.PointStyle,
                    Value = input.ColorMap is null ? null : c?[k]
                };
                points[k] = point;
            }

            // Generate the axes for the scatter plot when not specified.
            PlotAxis xAxis = input.XAxis ?? new PlotAxis() { Label = "X" };
            PlotAxis yAxis = input.YAxis ?? new PlotAxis() { Label = "Y" };
            PlotAxis zAxis = input.ZAxis ?? new PlotAxis() { Label = "Z" };

            // Create a new scatter plot structure.
            // Based on what the input data looks like, we'll generate a 2D or 3D scatter plot.
            ScatterPlot plot = new()
            {
                Data = points,
                Axes = new PlotAxes() { X = xAxis, Y = yAxis, Z = zAxis },
                Style = styles,
                Colormap = input.ColorMap,
            };
            return Task.FromResult(new ScatterPlotOperationOut() { Plot = plot });
        }
    }
}