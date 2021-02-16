using System;
using System.Diagnostics.CodeAnalysis;

namespace CartaCore.Data.Freeform
{
    /// <summary>
    /// Represents an identity that is compared on the basis of a constant value.
    /// </summary>
    /// <typeparam name="T">The type of equatable and comparable constant value.</typeparam>
    public class FreeformConstantIdentity<T> : FreeformIdentity
        where T : IEquatable<T>, IComparable<T>
    {
        /// <summary>
        /// Gets or sets the identity value.
        /// </summary>
        /// <value>The identitiy.</value>
        public T Identity { get; set; }

        /// <summary>
        /// Initializes an instance of the <see cref="FreeformConstantIdentity{T}"/> class with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier object.</param>
        public FreeformConstantIdentity(T id)
        {
            Identity = id;
        }

        /// <inheritdoc />
        public override bool Equals([AllowNull] FreeformIdentity other)
        {
            if (other is FreeformConstantIdentity<T> constantOther) return Identity.Equals(constantOther.Identity);
            else return other.Equals(this);
        }
        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is FreeformIdentity other) return Equals(other);
            return false;
        }

        /// <inheritdoc />
        public override int CompareTo([AllowNull] FreeformIdentity other)
        {
            if (other is FreeformConstantIdentity<T> constantOther) return Identity.CompareTo(constantOther.Identity);
            return other.CompareTo(this);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Identity.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Identity.ToString();
        }
    }
}