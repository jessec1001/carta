using System;

namespace CartaCore.Graph
{
    /// <summary>
    /// Represents an object that must be constructed with an identity that can be used to order and assert this object
    /// in relation to other objects of its kind.
    /// </summary>
    /// <typeparam name="T">The type of object that requires an identity.</typeparam>
    public abstract class Identifiable<T> : IIdentifiable, IEquatable<T>, IComparable<T> where T : Identifiable<T>
    {
        /// <summary>
        /// Gets or sets the identifier that uniquely represents this object. 
        /// </summary>
        /// <value>The identifier of this object. This value cannot be changed after initialization.</value>
        public string Id { get; init; }

        /// <summary>
        /// Initializes an instance of the <see cref="Identifiable{T}"/> class with its specified identifier.
        /// </summary>
        /// <param name="id">The identifier of this object.</param>
        protected Identifiable(string id) => Id = id;

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
        public static bool operator ==(Identifiable<T> lhs, Identifiable<T> rhs)
        {
            return lhs.Equals(rhs);
        }
        /// <summary>
        /// Determines whether the pair of identifiable objects are not equal.
        /// </summary>
        /// <param name="lhs">The first identifiable object.</param>
        /// <param name="rhs">The second identifiable object.</param>
        /// <returns><c>false</c> if the pair of objects are equal; otherwise, <c>true</c>.</returns>
        public static bool operator !=(Identifiable<T> lhs, Identifiable<T> rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}