using System.Collections.Generic;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a graph vertex that can be identified, and takes on properties with numerous observations.
    /// </summary>
    public class Vertex : Element<Vertex>, IVertex
    {
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