using System;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a property of a vertex.
    /// </summary>
    /// <seealso cref="FreeformVertex" />
    public class FreeformProperty
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value of the property.
        /// </value>
        public object Value { get; set; }
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type of the property.
        /// </value>
        public Type Type { get; set; }
    }
}