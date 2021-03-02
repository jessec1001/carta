using System.Collections.Generic;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a graph vertex that can be identified, has available out-edges, and takes on properties with numerous
    /// observations.
    /// </summary>
    public class OutVertex : Vertex, IOutVertex
    {
        /// <inheritdoc />
        public IEnumerable<Edge> OutEdges { get; protected init; }

        /// <summary>
        /// Initializes an instance of the <see cref="OutVertex"/> class with its specified identifier, a set of out-
        /// edges, and a set of properties assigned to it.
        /// </summary>
        /// <param name="id">The identifier of this vertex.</param>
        /// <param name="properties">The properties assigned to this vertex.</param>
        /// <param name="outEdges">
        /// The edges of this vertex with <see cref="Edge.Source"/> equal to this vertex's identifier.
        /// </param>
        public OutVertex(Identity id, IEnumerable<Property> properties, IEnumerable<Edge> outEdges)
            : base(id, properties)
        {
            OutEdges = outEdges;
        }
        /// <summary>
        /// Initializes an instance of the <see cref="OutVertex"/> class with its specified identifier and a set of out-
        /// edges.
        /// </summary>
        /// <param name="id">The identifier of this vertex.</param>
        /// <param name="outEdges">
        /// The edges of this vertex with <see cref="Edge.Source"/> equal to this vertex's identifier.
        /// </param>
        public OutVertex(Identity id, IEnumerable<Edge> outEdges)
            : base(id)
        {
            OutEdges = outEdges;
        }
        /// <summary>
        /// Initializes an instance of the <see cref="OutVertex"/> class from a copy of a vertex and a set of out-edges.
        /// </summary>
        /// <param name="vertex">The vertex to copy.</param>
        /// <param name="outEdges">
        /// The edges of this vertex with <see cref="Edge.Source"/> equal to this vertex's identifier.
        /// </param>
        public OutVertex(Vertex vertex, IEnumerable<Edge> outEdges)
            : base(vertex)
        {
            OutEdges = outEdges;
        }
    }
}