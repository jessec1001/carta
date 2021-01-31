using System;
using System.Collections.Generic;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a graph vertex that can take on multiple properties with multiple values.
    /// </summary>
    /// <seealso cref="FreeformId" />
    public class FreeformVertex : FreeformId
    {
        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label visible in visualizations of the vertex.
        /// </value>
        public string Label { get; set; }
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description of the vertex value presented as hover text in visualizations.
        /// </value>
        public string Description { get; set; }
        /// <summary>
        /// Gets or sets the properties.
        /// </summary>
        /// <value>
        /// The property values contained by the vertex.
        /// </value>
        public SortedList<string, FreeformProperty> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FreeformVertex"/> class with the specified ID.
        /// </summary>
        /// <param name="id">The vertex ID.</param>
        public FreeformVertex(Guid id) : base(id) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="FreeformVertex"/> class.
        /// </summary>
        public FreeformVertex() : base() { }
    }
}