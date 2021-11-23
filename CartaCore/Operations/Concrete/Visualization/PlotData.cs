namespace CartaCore.Operations.Visualization
{
    /// <summary>
    /// Represents a plot with some arbitrary type of data.
    /// </summary>
    /// <typeparam name="TData">The type of data in the plot.</typeparam>
    public struct PlotData<TData>
    {
        /// <summary>
        /// The list of data points in the plot.
        /// </summary>
        public TData[] Data { get; init; }
        /// <summary>
        /// The axes of the plot.
        /// </summary>
        public PlotAxes Axes { get; init; }

        /// <summary>
        /// If specified, represents the color of all values/elements in the visualization unless overridden by 
        /// individual data points.
        /// </summary>
        public string Color { get; init; }
        /// <summary>
        /// The name of the colormap to use for mapping numeric data to colors. If null, the default colormap is used.
        /// Will be overridden by the <see cref="Color"/> property or individual data points. 
        /// </summary>
        public string Colormap { get; init; }
    }
}