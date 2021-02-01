using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a graph vertex that can take on multiple properties with multiple values.
    /// </summary>
    public class FreeformVertex : IEquatable<FreeformVertex>, IComparable<FreeformVertex>
    {
        /// <summary>
        /// Gets or sets the ID of the vertex.
        /// </summary>
        /// <value>
        /// The vertex ID.
        /// </value>
        public Guid Id { get; set; }
        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label visible in visualizations of the vertex.
        /// </value>
        public string Label { get; set; }
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description of the vertex value presented as hover text in visualizations.
        /// </value>
        public string Description { get; set; }
        /// <summary>
        /// Gets or sets the properties.
        /// </summary>
        /// <value>
        /// The property values contained by the vertex.
        /// </value>
        public SortedList<string, FreeformProperty> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FreeformVertex"/> class with the specified ID.
        /// </summary>
        /// <param name="id">The vertex ID.</param>
        public FreeformVertex(Guid id) => Id = id;
        /// <summary>
        /// Initializes a new instance of the <see cref="FreeformVertex"/> class.
        /// </summary>
        public FreeformVertex() { }

        /// <inheritdoc />
        public bool Equals([AllowNull] FreeformVertex other)
        {
            if (other is null)
                return false;
            return Id.Equals(other.Id);
        }
        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is FreeformVertex other)
                return Equals(other);
            return false;
        }

        /// <inheritdoc />
        public int CompareTo([AllowNull] FreeformVertex other)
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
        public static bool operator ==(FreeformVertex lhs, FreeformVertex rhs)
        {
            return lhs.Equals(rhs);
        }
        /// <inheritdoc />
        public static bool operator !=(FreeformVertex lhs, FreeformVertex rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}