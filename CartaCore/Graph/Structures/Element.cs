using System;
using System.Collections.Generic;

namespace CartaCore.Graphs
{
    /// <summary>
    /// Represents a graph element that has a set of properties with numerous observations assigned to it.
    /// </summary>
    /// <typeparam name="T">The type of element.</typeparam>
    public abstract class Element<T> :
        Property,
        IIdentifiable,
        IElement,
        IEquatable<T>,
        IComparable<T>
        where T : Element<T>
    {
        /// <inheritdoc />
        public string Id { get; set; }

        /// <inheritdoc />
        public string Label { get; set; }
        /// <inheritdoc />
        public string Description { get; set; }

        /// <summary>
        /// Initializes an instance of the <see cref="Element{T}"/> class with its specified identifier and a set of
        /// properties assigned to it.
        /// </summary>
        /// <param name="id">The identifier of this element.</param>
        /// <param name="properties">The properties assigned to this element.</param>
        protected Element(string id, IDictionary<string, IProperty> properties)
        {
            // Assign the identifier.
            Id = id;

            // Check that properties is not null before assigning.
            if (properties is null) throw new ArgumentNullException(nameof(properties));
            Properties = properties;
        }
        /// <summary>
        /// Initializes an instance of the <see cref="Element{T}"/> class with its specified identifier.
        /// </summary>
        /// <param name="id">The identifier of this element.</param>
        protected Element(string id)
            : this(id, new Dictionary<string, IProperty>()) { }

        /// <inheritdoc />
        public int CompareTo(T other)
        {
            if (other is null) return 1;
            return Id.CompareTo(other.Id);
        }

        /// <inheritdoc />
        public bool Equals(T other)
        {
            if (other is null) return false;
            return Id.Equals(other.Id);
        }
        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is T other) return Equals(other);
            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Determines whether the pair of identifiable objects are equal.
        /// </summary>
        /// <param name="lhs">The first identifiable object.</param>
        /// <param name="rhs">The second identifiable object.</param>
        /// <returns><c>true</c> if the pair of objects are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Element<T> lhs, Element<T> rhs)
        {
            return lhs.Equals(rhs);
        }
        /// <summary>
        /// Determines whether the pair of identifiable objects are not equal.
        /// </summary>
        /// <param name="lhs">The first identifiable object.</param>
        /// <param name="rhs">The second identifiable object.</param>
        /// <returns><c>false</c> if the pair of objects are equal; otherwise, <c>true</c>.</returns>
        public static bool operator !=(Element<T> lhs, Element<T> rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}