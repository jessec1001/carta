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
        /// <returns>A graph of the children of the vertex. No edges are included in the graph.</returns>
        FreeformGraph GetChildren(Guid id)
        {
            // Create a graph with the correct directed variant.
            FreeformGraph graph;
            if (IsDirected)
                graph = new AdjacencyGraph<FreeformVertex, FreeformEdge>(true);
            else
                graph = new UndirectedGraph<FreeformVertex, FreeformEdge>(true);

            // Add children vertices.
            graph.AddVertexRange
            (
                GetEdges(id).Select(edge => GetProperties(edge.Target.Id))
            );

            return graph;
        }

        /// <summary>
        /// Gets the children of a vertex with their edge connects by ID.
        /// </summary>
        /// <param name="id">The ID of the vertex.</param>
        /// <returns>
        /// A graph of the children of the vertex. All in- and out-edges of the children are included in the graph.
        /// </returns>
        FreeformGraph GetChildrenWithEdges(Guid id)
        {
            // Create a graph with the correct directed variant.
            FreeformGraph graph;
            if (IsDirected)
                graph = new AdjacencyGraph<FreeformVertex, FreeformEdge>(true);
            else
                graph = new UndirectedGraph<FreeformVertex, FreeformEdge>(true);

            // Add children vertices.
            IEnumerable<FreeformEdge> edges = GetEdges(id);
            graph.AddVertexRange
            (
                edges.Select(edge => GetProperties(edge.Target.Id))
            );

            // Add parent-to-children edges.
            graph.AddVerticesAndEdgeRange
            (
                edges
                    .OrderBy(edge => edge.Target)
                    .Select((edge, index) =>
                        {
                            edge.Id = index;
                            return edge;
                        })
            );

            // Add children edges.
            graph.AddVerticesAndEdgeRange
            (
                edges.SelectMany(edge => GetEdges(edge.Target.Id)
                    .OrderBy(edge => edge.Target)
                    .Select((edge, index) =>
                        {
                            edge.Id = index;
                            return edge;
                        })
                    )
            );

            return graph;
        }
    }
}