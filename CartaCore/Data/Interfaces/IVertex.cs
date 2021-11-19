using System.Collections.Generic;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents the base functionality of a vertex.
    /// </summary>
    public interface IVertex : IElement<Vertex>
    {
        /// <summary>
        /// The edges that this vertex is a component of.
        /// </summary>
        IEnumerable<Edge> Edges { get; }
    }
}