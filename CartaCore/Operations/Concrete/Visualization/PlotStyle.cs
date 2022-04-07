using CartaCore.Operations.Attributes;

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
        [FieldName("Fill Color")]
        public string FillColor { get; set; }
        /// <summary>
        /// The fill radius of the element.
        /// </summary>
        [FieldName("Fill Radius")]
        public double? FillRadius { get; set; }

        /// <summary>
        /// The stroke color of the element.
        /// Must be one of the following:
        /// - A common color name such as "blue" or "tomato".
        /// - A hex encoded color in the form "#1267bc".
        /// </summary>
        [FieldName("Stroke Color")]
        public string StrokeColor { get; set; }
        /// <summary>
        /// The stroke width of the element.
        /// </summary>
        [FieldName("Stroke Width")]
        public double? StrokeWidth { get; set; }
    }
}