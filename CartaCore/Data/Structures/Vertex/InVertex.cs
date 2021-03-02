using System.Collections.Generic;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a graph vertex that can be identified, has available in-edges, and takes on properties with numerous
    /// observations.
    /// </summary>
    public class InVertex : Vertex, IInVertex
    {
        /// <inheritdoc />
        public IEnumerable<Edge> InEdges { get; protected init; }

        /// <summary>
        /// Initializes an instance of the <see cref="InVertex"/> class with its specified identifier, a set of in-
        /// edges, and a set of properties assigned to it.
        /// </summary>
        /// <param name="id">The identifier of this vertex.</param>
        /// <param name="properties">The properties assigned to this vertex.</param>
        /// <param name="inEdges">
        /// The edges of this vertex with <see cref="Edge.Target"/> equal to this vertex's identifier.
        /// </param>
        public InVertex(Identity id, IEnumerable<Property> properties, IEnumerable<Edge> inEdges)
            : base(id, properties)
        {
            InEdges = inEdges;
        }
        /// <summary>
        /// Initializes an instance of the <see cref="InVertex"/> class with its specified identifier and a set of in-
        /// edges.
        /// </summary>
        /// <param name="id">The identifier of this vertex.</param>
        /// <param name="inEdges">
        /// The edges of this vertex with <see cref="Edge.Target"/> equal to this vertex's identifier.
        /// </param>
        public InVertex(Identity id, IEnumerable<Edge> inEdges)
            : base(id)
        {
            InEdges = inEdges;
        }
        /// <summary>
        /// Initializes an instance of the <see cref="InVertex"/> class from a copy of a vertex and a set of in-edges.
        /// </summary>
        /// <param name="vertex">The vertex to copy.</param>
        /// <param name="inEdges">
        /// The edges of this vertex with <see cref="Edge.Target"/> equal to this vertex's identifier.
        /// </param>
        public InVertex(Vertex vertex, IEnumerable<Edge> inEdges)
            : base(vertex)
        {
            InEdges = inEdges;
        }
    }
}