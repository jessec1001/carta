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