using System.Collections.Generic;

namespace CartaCore.Operations.Visualization
{
    /// <summary>
    /// Represents a plot with some arbitrary type of data.
    /// </summary>
    public abstract class Plot
    {
        /// <summary>
        /// The type of plot. This should be consistent with the types of plots that are supported by the visualization
        /// library.
        /// </summary>
        public abstract string Type { get; }

        /// <summary>
        /// The title of the plot.
        /// </summary>
        public string Title { get; init; }
        /// <summary>
        /// The axes of the plot.
        /// </summary>
        public PlotAxes Axes { get; init; }

        /// <summary>
        /// The style properties of the plot.
        /// </summary>
        public Dictionary<string, string> Style { get; init; }
    }
}