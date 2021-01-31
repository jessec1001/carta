using System;
using System.Diagnostics.CodeAnalysis;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a freeform vertex containing only its ID.
    /// </summary>
    /// <seealso cref="FreeformVertex" />
    public class FreeformId : IEquatable<FreeformId>, IComparable<FreeformId>
    {
        /// <summary>
        /// Gets or sets the ID of the vertex.
        /// </summary>
        /// <value>
        /// The vertex ID.
        /// </value>
        public Guid Id { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FreeformId"/> class with the specified ID.
        /// </summary>
        /// <param name="id">The vertex ID.</param>
        public FreeformId(Guid id) => Id = id;
        /// <summary>
        /// Initializes a new instance of the <see cref="FreeformId"/> class.
        /// </summary>
        public FreeformId() { }

        /// <inheritdoc />
        public bool Equals([AllowNull] FreeformId other)
        {
            if (other is null)
                return false;
            return Id.Equals(other.Id);
        }
        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is FreeformId other)
                return Equals(other);
            return false;
        }

        /// <inheritdoc />
        public int CompareTo([AllowNull] FreeformId other)
        {
            if (other is null)
                return 1;
            return Id.CompareTo(other.Id);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <inheritdoc />
        public static bool operator ==(FreeformId lhs, FreeformId rhs)
        {
            return lhs.Equals(rhs);
        }
        /// <inheritdoc />
        public static bool operator !=(FreeformId lhs, FreeformId rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}