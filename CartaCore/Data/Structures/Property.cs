using System;
using System.Collections.Generic;
using System.Linq;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a property with any number of observations. This property may be attached to any element of a graph
    /// such as a vertex, an edge, or the graph itself.
    /// </summary>
    public class Property : Identifiable<Property>
    {
        /// <summary>
        /// Gets or sets the property observations.
        /// </summary>
        /// <value>The observations recorded for this property.</value>
        public IEnumerable<object> Values { get; set; }
        /// <summary>
        /// Gets or sets the subproperties.
        /// </summary>
        /// <value>The subproperties or metadata for this property.</value>
        public IEnumerable<Property> Subproperties { get; set; }

        /// <summary>
        /// Initializes an instance of the <see cref="Property"/> class with its specified identifier and a set of
        /// observations recorded for it.
        /// </summary>
        /// <param name="id">The identifier of this property.</param>
        /// <param name="values">The observations recorded for this property.</param>
        public Property(Identity id, IEnumerable<object> values)
            : base(id)
        {
            if (values is null) throw new ArgumentNullException(nameof(values));
            Values = values;
        }
        /// <summary>
        /// Initializes an instance of the <see cref="Property"/> class with its specified identifier.
        /// </summary>
        /// <param name="id">The identifier of this property.</param>
        public Property(Identity id)
            : this(id, Enumerable.Empty<object>()) { }
    }
}