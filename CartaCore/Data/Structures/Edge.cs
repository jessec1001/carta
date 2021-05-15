using System;
using System.Collections.Generic;
using System.Linq;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a graph edge that connects a pair of vertices that can be identified, and takes on properties with
    /// numerous observations.
    /// </summary>
    public class Edge : Element<Edge>
    {
        /// <summary>
        /// Gets the identifier of the source vertex.
        /// </summary>
        /// <value>The source vertex identifier.</value>
        public Identity Source { get; protected init; }
        /// <summary>
        /// Gets the identifier of the target vertex.
        /// </summary>
        /// <value>The target vertex identifier.</value>
        public Identity Target { get; protected init; }


        public Edge(Identity id, Identity source, Identity target, IEnumerable<Property> properties)
            : base(id, properties)
        {
            Source = source;
            Target = target;
        }
        /// <summary>
        /// Initializes an instance of the <see cref="Edge"/> class with the specified source and target vertex
        /// identifiers and assigned properties.
        /// </summary>
        /// <param name="source">The source vertex identifier.</param>
        /// <param name="target">The target vertex identifier.</param>
        /// <param name="properties">The properties assigned to the edge.</param>
        public Edge(Identity source, Identity target, IEnumerable<Property> properties)
            : base(Identity.Create(new EdgeIdentifier(source, target)), properties)
        {
            Source = source;
            Target = target;
        }
        /// <summary>
        /// Initializes an instance of the <see cref="Edge"/> class with the specified source and target vertex.
        /// </summary>
        /// <param name="source">The source vertex identifier.</param>
        /// <param name="target">The target vertex identifier.</param>
        public Edge(Identity source, Identity target)
            : this(source, target, Enumerable.Empty<Property>()) { }
        /// <summary>
        /// Initializes an instance of the <see cref="Edge"/> class with the specified source and target vertices and
        /// assigned properties.
        /// </summary>
        /// <param name="source">The source vertex.</param>
        /// <param name="target">The target vertex.</param>
        /// <param name="properties">The properties assigned to the edge.</param>
        public Edge(Vertex source, Vertex target, IEnumerable<Property> properties)
            : this(source.Identifier, target.Identifier, properties) { }
        /// <summary>
        /// Initializes an instance of the <see cref="Edge"/> class with the specified source and target vertices.
        /// </summary>
        /// <param name="source">The source vertex.</param>
        /// <param name="target">The target vertex.</param>
        public Edge(Vertex source, Vertex target)
            : this(source.Identifier, target.Identifier) { }
    }

    /// <summary>
    /// Represents a unique identifier of an edge using its source and target vertices. Note that this identifier for an
    /// edge does not support uniqueness among parallel edges.
    /// </summary>
    public class EdgeIdentifier : IEquatable<EdgeIdentifier>, IComparable<EdgeIdentifier>
    {
        /// <summary>
        /// The source identity that is used to construct the identifier.
        /// </summary>
        private Identity Source;
        /// <summary>
        /// The target identity that is used to construct the identifier.
        /// </summary>
        private Identity Target;

        /// <summary>
        /// Initializes an instance of the <see cref="EdgeIdentifier"/> class with the specified source and target
        /// vertex identifiers.
        /// </summary>
        /// <param name="source">The source vertex identifier.</param>
        /// <param name="target">The target vertex identifier.</param>
        public EdgeIdentifier(Identity source, Identity target)
        {
            Source = source;
            Target = target;
        }

        /// <inheritdoc />
        public int CompareTo(EdgeIdentifier other)
        {
            // Compare source first and target second.
            int compareSource = Source.CompareTo(other.Source);
            int compareTarget = Target.CompareTo(other.Target);
            return compareSource == 0 ? compareTarget : compareSource;
        }

        /// <inheritdoc />
        public bool Equals(EdgeIdentifier other)
        {
            return Source == other.Source && Target == other.Target;
        }
        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is EdgeIdentifier other) return Equals(other);
            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (Source, Target).GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Source}::{Target}";
        }
    }
}