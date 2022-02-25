using System.Collections.Generic;
using System.Linq;

namespace CartaCore.Graph
{
    /// <summary>
    /// Represents the base functionality of a vertex.
    /// </summary>
    /// <typeparam name="TEdge">The type of edge.</typeparam>
    public interface IVertex<out TEdge> : IIdentifiable where TEdge : IEdge
    {
        /// <summary>
        /// The edges that this vertex is a component of.
        /// </summary>
        IEnumerable<TEdge> Edges { get; }
        /// <summary>
        /// The in-edges pointing from this vertex.
        /// </summary>
        IEnumerable<TEdge> InEdges => Edges.Where(edge => edge.Target == Id);
        /// <summary>
        /// The out-edges pointing to this vertex.
        /// </summary>
        IEnumerable<TEdge> OutEdges => Edges.Where(edge => edge.Source == Id);
    }
    /// <summary>
    /// Represents the base functionality of a vertex with a specific type of edge.
    /// </summary>
    public interface IVertex : IVertex<IEdge> { }
}