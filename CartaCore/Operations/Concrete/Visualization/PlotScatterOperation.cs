using System;
using System.Drawing;
using System.Threading.Tasks;
using CartaCore.Operations.Visualization;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="ScatterPlotOperation" /> operation.
    /// </summary>
    public struct ScatterPlotOperationIn
    {
        /// <summary>
        /// The name of the output visualization.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The x-values for the points in the scatter plot. 
        /// </summary>
        public double[] X { get; set; }
        /// <summary>
        /// The y-values for the points in the scatter plot.
        /// </summary>
        public double[] Y { get; set; }
        /// <summary>
        /// The z-values for the points in the scatter plot.
        /// If not specified, will be a 2D scatter plot.
        /// </summary>
        public double[] Z { get; set; }

        /// <summary>
        /// The radii of the points in the scatter plot.
        /// </summary>
        public double[] Radii { get; set; }

        /// <summary>
        /// The color to apply to points in the scatter plot.
        /// Must be one of the following:
        /// - A common color name such as "blue" or "tomato".
        /// - A hex encoded color in the form "#1267bc".
        /// </summary>
        public string Color { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="ScatterPlotOperation" /> operation.
    /// </summary>
    public struct ScatterPlotOperationOut { }


    /// <summary>
    /// Visualizes a scatter plot of 2D or 3D data.
    /// </summary>
    public class ScatterPlotOperation : TypedOperation
    <
        ScatterPlotOperationIn,
        ScatterPlotOperationOut
    >
    {
        /// <summary>
        /// The value of a scatter plot point.
        /// </summary>
        private struct ScatterPlotValue
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
            public double Radius { get; set; }

            /// <summary>
            /// The color of the scatter plot point.
            /// </summary>
            public string Color { get; set; }
        }

        /// <inheritdoc />
        public override Task<ScatterPlotOperationOut> Perform(
            ScatterPlotOperationIn input,
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

            // Get the data for the scatter plot. This format should be compliant with the visualization library so that
            // it may be used directly from the front-end to generate visualizations.
            // Based on what the input data looks like, we'll generate a 2D or 3D scatter plot.
            PlotData<ScatterPlotValue> scatterPlot;

            // Gather the data.
            double[] x = input.X;
            double[] y = input.Y;
            double[] z = input.Z;
            double[] radii = input.Radii;

            // Check the data lengths.
            int length = 0;
            if (x is not null && y is not null)
            {
                if (x.Length != y.Length) throw new ArgumentException("All of the data arrays must be the same length.");
                else length = x.Length;
            }
            if (y is not null && z is not null)
            {
                if (y.Length != z.Length) throw new ArgumentException("All of the data arrays must be the same length.");
                else length = y.Length;
            }
            if (z is not null && x is not null)
            {
                if (z.Length != x.Length) throw new ArgumentException("All of the data arrays must be the same length.");
                else length = z.Length;
            }
            if (radii is not null)
            {
                if (radii.Length != length) throw new ArgumentException("All of the data arrays must be the same length.");
            }

            // Create the scatter plot data.
            ScatterPlotValue[] values = new ScatterPlotValue[length];
            for (int k = 0; k < length; k++)
            {
                values[k] = new ScatterPlotValue
                {
                    X = x?[k],
                    Y = y?[k],
                    Z = z?[k],
                    Radius = radii?[k] ?? 1.0,
                    Color = color.Name
                };
            }
            scatterPlot = new()
            {
                Data = values,
                Axes = new PlotAxes()
                {
                    X = x is null ? null : new PlotAxis() { Label = "X Values" },
                    Y = y is null ? null : new PlotAxis() { Label = "Y Values" },
                    Z = z is null ? null : new PlotAxis() { Label = "Z Values" }
                }
            };

            // Output the visualization data to the calling context.
            if (callingContext is not null && callingContext.Output.TryAdd(input.Name, scatterPlot))
                return Task.FromResult(new ScatterPlotOperationOut());
            else
                throw new ArgumentException($"Cannot set visualization '{input.Name}'.");
        }
    }
}