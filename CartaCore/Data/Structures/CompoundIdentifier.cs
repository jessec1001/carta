using System;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents an identifier that has a parent object and an auxilliary identifier combined into a single object.
    /// </summary>
    /// <typeparam name="T">The type of parent object.</typeparam>
    public class CompoundIdentifier<T> : IEquatable<CompoundIdentifier<T>>, IComparable<CompoundIdentifier<T>>
        where T : Identifiable<T>
    {
        /// <summary>
        /// Gets the parent identifiable object.
        /// </summary>
        /// <value>
        /// The parent object which is compared and checked for equality before the auxilliary identifier.
        /// </value>
        public T Parent { get; protected init; }
        /// <summary>
        /// Gets the auxilliary identifier.
        /// </summary>
        /// <value>
        /// The auxilliary identifier which is compared and checked for equality after the parent object.
        /// </value>
        public Identity Identifier { get; protected init; }

        /// <summary>
        /// Initializes an instance of the <see cref="CompoundIdentifier{T}"/> class with the specified parent
        /// identifiable object and an auxilliary identifier.
        /// </summary>
        /// <param name="parent">The parent object.</param>
        /// <param name="id">The auxilliary identifier.</param>
        public CompoundIdentifier(T parent, Identity id)
        {
            // Check for null values before setting the arguments.
            if (parent is null) throw new ArgumentNullException(nameof(parent));
            if (id is null) throw new ArgumentNullException(nameof(id));

            Parent = parent;
            Identifier = id;
        }

        /// <inheritdoc />
        public int CompareTo(CompoundIdentifier<T> other)
        {
            if (other is null) return 1;

            // Compare the parent first, and the auxilliary identifier second.
            int compareParent = Parent.CompareTo(other.Parent);
            if (compareParent == 0)
            {
                int compareIdentifier = Identifier.CompareTo(other.Identifier);
                return compareIdentifier;
            }
            else return compareParent;
        }

        /// <inheritdoc />
        public bool Equals(CompoundIdentifier<T> other)
        {
            if (other is null) return false;
            return Parent.Equals(other.Parent) && Identifier.Equals(other.Identifier);
        }
        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is CompoundIdentifier<T> other) return Equals(other);
            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (Parent.Identifier, Identifier).GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Parent.Identifier}::{Identifier}";
        }
    }
}