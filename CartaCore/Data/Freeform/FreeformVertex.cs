using System.Collections.Generic;

namespace CartaCore.Data.Freeform
{
    /// <summary>
    /// Represents a graph vertex that can be identified, and takes on multiple properties that may contain numerous
    /// observations.
    /// </summary>
    public class FreeformVertex : FreeformObjectBase<FreeformVertex>
    {

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>The label visible in visualizations of the vertex.</value>
        public string Label { get; set; }
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description of the vertex visibile in visualizations of the vertex.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the properties.
        /// </summary>
        /// <value>The observed properties assigned to the vertex.</value>
        public IEnumerable<FreeformProperty> Properties { get; set; }

        /// <summary>
        /// Initializes an instance of the <see cref="FreeformVertex"/> class with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public FreeformVertex(FreeformIdentity id)
            : base(id) { }
    }
}