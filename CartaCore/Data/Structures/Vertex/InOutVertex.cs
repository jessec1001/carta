using System.Collections.Generic;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a graph vertex that can be identified, has available in- and out-edges, and takes on properties with
    /// numerous observations.
    /// </summary>
    public class InOutVertex : Vertex, IInVertex, IOutVertex
    {
        /// <inheritdoc />
        public IEnumerable<Edge> InEdges { get; protected init; }
        /// <inheritdoc />
        public IEnumerable<Edge> OutEdges { get; protected init; }

        /// <summary>
        /// Initializes an instance of the <see cref="InOutVertex"/> class with its specified identifier, a set of in-
        /// and out-edges, and a set of properties assigned to it.
        /// </summary>
        /// <param name="id">The identifier of this vertex.</param>
        /// <param name="properties">The properties assigned to this vertex.</param>
        /// <param name="inEdges">
        /// The edges of this vertex with <see cref="Edge.Target"/> equal to this vertex's identifier.
        /// </param>
        /// <param name="outEdges">
        /// The edges of this vertex with <see cref="Edge.Source"/> equal to this vertex's identifier.
        /// </param>
        public InOutVertex(
            Identity id, IEnumerable<Property> properties,
            IEnumerable<Edge> inEdges, IEnumerable<Edge> outEdges
        ) : base(id, properties)
        {
            InEdges = inEdges;
            OutEdges = outEdges;
        }
        /// <summary>
        /// Initializes an instance of the <see cref="InOutVertex"/> class with its specified identifier and a set of
        /// in- and out-edges.
        /// </summary>
        /// <param name="id">The identifier of this vertex.</param>
        /// <param name="inEdges">
        /// The edges of this vertex with <see cref="Edge.Target"/> equal to this vertex's identifier.
        /// </param>
        /// <param name="outEdges">
        /// The edges of this vertex with <see cref="Edge.Source"/> equal to this vertex's identifier.
        /// </param>
        public InOutVertex(Identity id, IEnumerable<Edge> inEdges, IEnumerable<Edge> outEdges)
            : base(id)
        {
            InEdges = inEdges;
            OutEdges = outEdges;
        }
        /// <summary>
        /// Initializes an instance of the <see cref="InOutVertex"/> class from a copy of a vertex and a set of in- and
        /// out-edges.
        /// </summary>
        /// <param name="vertex">The vertex to copy.</param>
        /// <param name="inEdges">
        /// The edges of this vertex with <see cref="Edge.Target"/> equal to this vertex's identifier.
        /// </param>
        /// <param name="outEdges">
        /// The edges of this vertex with <see cref="Edge.Source"/> equal to this vertex's identifier.
        /// </param>
        public InOutVertex(Vertex vertex, IEnumerable<Edge> inEdges, IEnumerable<Edge> outEdges)
            : base(vertex)
        {
            InEdges = inEdges;
            OutEdges = outEdges;
        }
    }
}