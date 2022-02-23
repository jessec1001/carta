using System.Collections.Generic;
using System.Linq;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a graph vertex that can be identified, and takes on properties with numerous observations.
    /// </summary>
    /// <typeparam name="TEdge">The type of edge that this vertex is connected to.</typeparam>
    public class Vertex<TEdge> : Element<Vertex<TEdge>>, IVertex<TEdge> where TEdge : IEdge
    {
        /// <inheritdoc />
        public IEnumerable<TEdge> Edges { get; protected init; } = Enumerable.Empty<TEdge>();

        /// <summary>
        /// Initializes an instance of the <see cref="Vertex"/> class with its specified identifier, a set of properties
        /// assigned to it, and a set of edges that it is a component of.
        /// </summary>
        /// <param name="id">The identifier of this vertex.</param>
        /// <param name="properties">The properties assigned to this vertex.</param>
        /// <param name="edges">The edges that the vertex is adjacent to.</param>
        public Vertex(string id, ISet<IProperty> properties, IEnumerable<TEdge> edges)
            : base(id, properties)
        {
            Edges = edges;
        }
        /// <summary>
        /// Initializes an instance of the <see cref="Vertex"/> class with its specified identifier and a set of edges
        /// that it is a component of.
        /// </summary>
        /// <param name="id">The identifier of this vertex.</param>
        /// <param name="edges">The edges that the vertex is adjacent to.</param>
        public Vertex(string id, IEnumerable<TEdge> edges)
            : base(id)
        {
            Edges = edges;
        }
        /// <summary>
        /// Initializes an instance of the <see cref="Vertex"/> class with its specified identifier and a set of
        /// properties assigned to it.
        /// </summary>
        /// <param name="id">The identifier of this vertex.</param>
        /// <param name="properties">The properties assigned to this vertex.</param>
        public Vertex(string id, ISet<IProperty> properties)
            : base(id, properties) { }
        /// <summary>
        /// Initializes an instance of the <see cref="Vertex"/> class with its specified identifier.
        /// </summary>
        /// <param name="id">The identifier of this vertex.</param>
        public Vertex(string id)
            : base(id) { }
        /// <summary>
        /// Initializes an instance of the <see cref="Vertex"/> as a copy of the specified vertex.
        /// </summary>
        /// <param name="vertex">The vertex to copy.</param>
        public Vertex(Vertex vertex)
            : base(vertex.Id, vertex.Properties)
        {
            Label = vertex.Label;
            Description = vertex.Description;
        }
    }

    /// <summary>
    /// Represents a graph vertex that can be identified, and takes on properties with numerous observations.
    /// </summary>
    public class Vertex : Vertex<Edge>
    {
        /// <summary>
        /// Initializes an instance of the <see cref="Vertex"/> class with its specified identifier, a set of properties
        /// assigned to it, and a set of edges that it is a component of.
        /// </summary>
        /// <param name="id">The identifier of this vertex.</param>
        /// <param name="properties">The properties assigned to this vertex.</param>
        /// <param name="edges">The edges that the vertex is adjacent to.</param>
        public Vertex(string id, ISet<IProperty> properties, IEnumerable<Edge> edges)
            : base(id, properties, edges) { }
        /// <summary>
        /// Initializes an instance of the <see cref="Vertex"/> class with its specified identifier and a set of edges
        /// that it is a component of.
        /// </summary>
        /// <param name="id">The identifier of this vertex.</param>
        /// <param name="edges">The edges that the vertex is adjacent to.</param>
        public Vertex(string id, IEnumerable<Edge> edges)
            : base(id, edges) { }
        /// <summary>
        /// Initializes an instance of the <see cref="Vertex"/> class with its specified identifier and a set of
        /// properties assigned to it.
        /// </summary>
        /// <param name="id">The identifier of this vertex.</param>
        /// <param name="properties">The properties assigned to this vertex.</param>
        public Vertex(string id, ISet<IProperty> properties)
            : base(id, properties) { }
        /// <summary>
        /// Initializes an instance of the <see cref="Vertex"/> class with its specified identifier.
        /// </summary>
        /// <param name="id">The identifier of this vertex.</param>
        public Vertex(string id)
            : base(id) { }
        /// <summary>
        /// Initializes an instance of the <see cref="Vertex"/> as a copy of the specified vertex.
        /// </summary>
        /// <param name="vertex">The vertex to copy.</param>
        public Vertex(Vertex vertex)
            : base(vertex) { }
    }
}