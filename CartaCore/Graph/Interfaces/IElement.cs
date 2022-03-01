namespace CartaCore.Graphs
{
    /// <summary>
    /// Represents the base structure of a graph element with properties assigned to it.
    /// </summary>
    public interface IElement : IIdentifiable, IProperty
    {
        /// <summary>
        /// The label visible in visualizations of the element.
        /// </summary>
        string Label { get; set; }
        /// <summary>
        /// The description of the element visibile in visualizations of the element.
        /// </summary>
        string Description { get; set; }
    }
}