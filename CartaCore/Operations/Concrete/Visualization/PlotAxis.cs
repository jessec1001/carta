namespace CartaCore.Operations.Visualization
{
    /// <summary>
    /// Information for a plot axis.
    /// </summary>
    public struct PlotAxis
    {
        /// <summary>
        /// The label of the axis.
        /// </summary>
        public string Label { get; init; }

        /// <summary>
        /// Whether to display grid lines on the axis.
        /// </summary>
        public bool ShowLines { get; set; }

        /// <summary>
        /// The minimum value of the axis. If not specified, defaults to the minimum extent of the data.
        /// </summary>
        public double? Minimum { get; set; }
        /// <summary>
        /// The maximum value of the axis. If not specified, defaults to the maximum extent of the data.
        /// </summary>
        public double? Maximum { get; set; }
    }

    /// <summary>
    /// Information for a set of plot axes.
    /// </summary>
    public struct PlotAxes
    {
        /// <summary>
        /// The x-axis for a plot.
        /// </summary>
        public PlotAxis? X { get; init; }
        /// <summary>
        /// The y-axis for a plot.
        /// </summary>
        public PlotAxis? Y { get; init; }
        /// <summary>
        /// The z-axis for a plot.
        /// </summary>
        public PlotAxis? Z { get; init; }
    }
}