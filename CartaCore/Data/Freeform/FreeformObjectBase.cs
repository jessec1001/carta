using System;
using System.Diagnostics.CodeAnalysis;

namespace CartaCore.Data.Freeform
{
    /// <summary>
    /// The base freeform object for other freeform objects to inherit from.
    /// </summary>
    /// <typeparam name="T">The type of object.</typeparam>
    public abstract class FreeformObjectBase<T> : IEquatable<T>, IComparable<T> where T : FreeformObjectBase<T>
    {
        /// <summary>
        /// Gets or sets the primary identifier of the freeform object.
        /// </summary>
        /// <value>The primary identifier.</value>
        public FreeformIdentity Identifier { get; set; }

        /// <summary>
        /// Initializes an instance of the <see cref="FreeformObjectBase{T}"/> class with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public FreeformObjectBase(FreeformIdentity id)
        {
            Identifier = id;
        }

        /// <inheritdoc />
        public bool Equals([AllowNull] T other)
        {
            if (other is null) return false;
            return Identifier.Equals(other.Identifier);
        }
        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is T other) return Equals(other);
            return false;
        }

        /// <inheritdoc />
        public int CompareTo([AllowNull] T other)
        {
            if (other is null) return 1;
            return Identifier.CompareTo(other.Identifier);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Identifier.GetHashCode();
        }

        /// <inheritdoc />
        public static bool operator ==(FreeformObjectBase<T> lhs, FreeformObjectBase<T> rhs)
        {
            return lhs.Equals(rhs);
        }
        /// <inheritdoc />
        public static bool operator !=(FreeformObjectBase<T> lhs, FreeformObjectBase<T> rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}