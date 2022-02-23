using System.Collections.Generic;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a graph edge that connects a pair of vertices that can be identified, and takes on properties with
    /// numerous observations.
    /// </summary>
    public class Edge : Element<Edge>, IEdge
    {
        /// <inheritdoc />
        public bool Directed { get; init; } = true;

        /// <summary>
        /// The weight of this edge. Defaults to 1.
        /// </summary>
        public double Weight { get; init; } = 1.0;

        /// <inheritdoc />
        public string Source { get; protected init; }
        /// <inheritdoc />
        public string Target { get; protected init; }

        /// <summary>
        /// Initializates an instance of the <see cref="Edge"/> class with the specified identity, source and target
        /// vertex identifiers and assigned properties.
        /// </summary>
        /// <param name="id">The edge identifier.</param>
        /// <param name="source">The source vertex identifier.</param>
        /// <param name="target">The target vertex identifier.</param>
        /// <param name="properties">The properties assigned to the edge.</param>
        public Edge(string id, string source, string target, ISet<IProperty> properties)
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
        public Edge(string source, string target, ISet<IProperty> properties)
            : base($"{source}::{target}", properties)
        {
            Source = source;
            Target = target;
        }
        /// <summary>
        /// Initializes an instance of the <see cref="Edge"/> class with the specified source and target vertex.
        /// </summary>
        /// <param name="source">The source vertex identifier.</param>
        /// <param name="target">The target vertex identifier.</param>
        public Edge(string source, string target)
            : this(source, target, new HashSet<IProperty>()) { }
        /// <summary>
        /// Initializes an instance of the <see cref="Edge"/> class with the specified source and target vertices and
        /// assigned properties.
        /// </summary>
        /// <param name="source">The source vertex.</param>
        /// <param name="target">The target vertex.</param>
        /// <param name="properties">The properties assigned to the edge.</param>
        public Edge(Vertex source, Vertex target, ISet<IProperty> properties)
            : this(source.Id, target.Id, properties) { }
        /// <summary>
        /// Initializes an instance of the <see cref="Edge"/> class with the specified source and target vertices.
        /// </summary>
        /// <param name="source">The source vertex.</param>
        /// <param name="target">The target vertex.</param>
        public Edge(Vertex source, Vertex target)
            : this(source.Id, target.Id) { }
    }
}