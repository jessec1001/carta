using System.Collections.Generic;
using System.Linq;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a graph vertex that can be identified, and takes on properties with numerous observations.
    /// </summary>
    public class Vertex : Element<Vertex>, IVertex
    {
        /// <inheritdoc />
        public IEnumerable<Edge> Edges { get; protected init; } = Enumerable.Empty<Edge>();

        // TODO: Fix the long-lasting problem where we cannot use the double equals operator on identifiers.
        /// <summary>
        /// The in-edges pointing from this vertex.
        /// </summary>
        public IEnumerable<Edge> InEdges => Edges.Where(e => e.Target.Equals(Identifier));
        /// <summary>
        /// The out-edges pointing to this vertex.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Edge> OutEdges => Edges.Where(e => e.Source.Equals(Identifier));

        /// <summary>
        /// Initializes an instance of the <see cref="Vertex"/> class with its specified identifier, a set of properties
        /// assigned to it, and a set of edges that it is a component of.
        /// </summary>
        /// <param name="id">The identifier of this vertex.</param>
        /// <param name="properties">The properties assigned to this vertex.</param>
        /// <param name="edges">The edges that the vertex is adjacent to.</param>
        public Vertex(Identity id, IEnumerable<Property> properties, IEnumerable<Edge> edges)
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
        public Vertex(Identity id, IEnumerable<Edge> edges)
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
        public Vertex(Identity id, IEnumerable<Property> properties)
            : base(id, properties) { }
        /// <summary>
        /// Initializes an instance of the <see cref="Vertex"/> class with its specified identifier.
        /// </summary>
        /// <param name="id">The identifier of this vertex.</param>
        public Vertex(Identity id)
            : base(id) { }
        /// <summary>
        /// Initializes an instance of the <see cref="Vertex"/> as a copy of the specified vertex.
        /// </summary>
        /// <param name="vertex">The vertex to copy.</param>
        public Vertex(Vertex vertex)
            : base(vertex.Identifier, vertex.Properties)
        {
            Label = vertex.Label;
            Description = vertex.Description;
        }
    }
}