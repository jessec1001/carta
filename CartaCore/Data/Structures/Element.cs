using System;
using System.Collections.Generic;
using System.Linq;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a graph element that has a set of properties with numerous observations assigned to it.
    /// </summary>
    /// <typeparam name="T">The type of element.</typeparam>
    public abstract class Element<T> : Identifiable<T>, IElement<T> where T : Element<T>
    {
        /// <inheritdoc />
        public string Label { get; set; }
        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public IEnumerable<Property> Properties { get; protected init; }

        /// <summary>
        /// Initializes an instance of the <see cref="Element{T}"/> class with its specified identifier and a set of
        /// properties assigned to it.
        /// </summary>
        /// <param name="id">The identifier of this element.</param>
        /// <param name="properties">The properties assigned to this element.</param>
        protected Element(Identity id, IEnumerable<Property> properties)
            : base(id)
        {
            // Check that properties is not null before assigning.
            if (properties is null) throw new ArgumentNullException(nameof(properties));
            Properties = properties;
        }
        /// <summary>
        /// Initializes an instance of the <see cref="Element{T}"/> class with its specified identifier.
        /// </summary>
        /// <param name="id">The identifier of this element.</param>
        protected Element(Identity id)
            : this(id, Enumerable.Empty<Property>()) { }
    }
}