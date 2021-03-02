using System.Collections.Generic;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a graph element that has a set of properties with numerous observations assigned to it.
    /// </summary>
    /// <typeparam name="T">The type of element.</typeparam>
    public interface IElement<T> : IIdentifiable<T> where T : IElement<T>
    {
        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>The label visible in visualizations of the element.</value>
        string Label { get; set; }
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description of the element visibile in visualizations of the element.</value>
        string Description { get; set; }

        /// <summary>
        /// Gets or sets the properties.
        /// </summary>
        /// <value>The observed properties assigned to the element.</value>
        IEnumerable<Property> Properties { get; }
    }
}