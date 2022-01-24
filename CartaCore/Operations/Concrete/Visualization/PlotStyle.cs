namespace CartaCore.Operations.Visualization
{
    /// <summary>
    /// Information about how to style a plot element.
    /// </summary>
    public struct PlotStyle
    {
        /// <summary>
        /// The fill color of the element.
        /// Must be one of the following:
        /// - A common color name such as "blue" or "tomato".
        /// - A hex encoded color in the form "#1267bc".
        /// </summary>
        public string FillColor { get; set; }
        /// <summary>
        /// The fill radius of the element.
        /// </summary>
        public double? FillRadius { get; set; }

        /// <summary>
        /// The stroke color of the element.
        /// Must be one of the following:
        /// - A common color name such as "blue" or "tomato".
        /// - A hex encoded color in the form "#1267bc".
        /// </summary>
        public string StrokeColor { get; set; }
        /// <summary>
        /// The stroke width of the element.
        /// </summary>
        public double? StrokeWidth { get; set; }
    }
}