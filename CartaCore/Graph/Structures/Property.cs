using System.Collections.Generic;

namespace CartaCore.Graphs
{
    /// <summary>
    /// Represents a property with any number of observations. This property may be attached to any element of a graph
    /// such as a vertex, an edge, or the graph itself.
    /// </summary>
    public class Property : IProperty
    {
        /// <inheritdoc />
        public virtual object Value { get; set; }

        /// <inheritdoc />
        public IDictionary<string, IProperty> Properties { get; init; }

        /// <summary>
        /// Initializes an instance of the <see cref="Property"/> class with a specified value.
        /// </summary>
        /// <param name="value">The observation recorded for this property.</param>
        public Property(object value)
        {
            Value = value;
        }
        /// <summary>
        /// Initializes an instance of the <see cref="Property"/> class with no value.
        /// </summary>
        public Property()
            : this(null) { }
    }
}