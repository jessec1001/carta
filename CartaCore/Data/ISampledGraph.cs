using System;
using System.Collections.Generic;

using QuikGraph;

namespace CartaCore.Data
{
    using FreeformGraph = IEdgeListAndIncidenceGraph<FreeformVertex, Edge<FreeformVertex>>;

    /// <summary>
    /// Represents graph data that can be queried vertex-by-vertex.
    /// </summary>
    public interface ISampledGraph
    {
        /// <summary>
        /// Whether the graph has a finite or infinite number of vertices and edges.
        /// </summary>
        /// <value><c>true</c> if the graph is finite; otherwise, <c>false</c>.</value>
        bool IsFinite { get; }

        /// <summary>
        /// Gets the entirety of the graph data.
        /// </summary>
        /// <remarks>
        /// This method should only work when <see cref="IsFinite"/> is <c>true</c>.
        /// </remarks>
        /// <returns>A freeform graph containing all vertices and edges.</returns>
        FreeformGraph GetEntire();

        /// <summary>
        /// Gets the properties of a vertex specified by ID.
        /// </summary>
        /// <param name="id">The ID of the vertex.</param>
        /// <returns>A vertex with its properties filled out.</returns>
        FreeformVertex GetProperties(Guid id);
        /// <summary>
        /// Gets the out-edges of a vertex specified by ID.
        /// </summary>
        /// <remarks>
        /// None of the vertices in the edges will contain any properties. Additionally, the
        /// <see cref="Edge{FreeformVertex}.Source"/> of each edge will always be the vertex with specified
        /// <paramref name="id"/>.
        /// </remarks>
        /// <param name="id">The ID of the vertex.</param>
        /// <returns>An enumerable of the out-edges of the vertex.</returns>
        IEnumerable<Edge<FreeformVertex>> GetEdges(Guid id);
    }
}