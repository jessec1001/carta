using System;
using System.Collections.Generic;
using System.Linq;

using QuikGraph;

namespace CartaCore.Data
{
    using FreeformGraph = IMutableVertexAndEdgeSet<FreeformVertex, FreeformEdge>;

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
        /// Whether the graph has directed or undirected edges.
        /// </summary>
        /// <value><c>true</c> if the graph has directed edges; otherwise, <c>false</c>.</value>
        bool IsDirected { get; }
        /// <summary>
        /// The ID of the base vertex of the graph.
        /// </summary>
        /// <remarks>
        /// This is required to provide deterministic behavior in infinite graphs.
        /// </remarks>
        /// <value>A GUID of a base vertex in the graph.</value>
        Guid BaseId { get; }

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
        IEnumerable<FreeformEdge> GetEdges(Guid id);

        /// <summary>
        /// Gets the children of a vertex specified by ID.
        /// </summary>
        /// <param name="id">The ID of the vertex.</param>
        /// <returns>A dictionary of ID-vertex pairs of children of the vertex.</returns>
        IDictionary<Guid, FreeformVertex> GetChildren(Guid id)
        {
            return GetEdges(id)
                .ToDictionary(
                    edge => edge.Target.Id,
                    edge => GetProperties(edge.Target.Id)
                );
        }
    }
}