using System;
using System.Collections.Generic;
using System.Linq;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a property with any number of observations. This property may be attached to any element of a graph
    /// such as a vertex, an edge, or the graph itself.
    /// </summary>
    public class Property : Element<Property>, IProperty
    {
        /// <summary>
        /// Initializes an instance of the <see cref="Property"/> class with its specified identifier and a set of
        /// observations recorded for it.
        /// </summary>
        /// <param name="id">The identifier of this property.</param>
        /// <param name="value">The observation recorded for this property.</param>
        public Property(string id, object value)
            : base(id)
        {
            Value = value;
        }
        /// <summary>
        /// Initializes an instance of the <see cref="Property"/> class with its specified identifier.
        /// </summary>
        /// <param name="id">The identifier of this property.</param>
        public Property(string id)
            : this(id, null) { }
    }
}