using System;
using System.Collections.Generic;
using System.Linq;

using QuikGraph;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a sampled graph that caches all results that it is requested.
    /// </summary>
    public class CachedGraph : ISampledGraph
    {
        private ISampledGraph Graph { get; set; }
        private Dictionary<Guid, FreeformVertex> Vertices { get; set; }
        private Dictionary<Guid, List<FreeformEdge>> Edges { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedGraph"/> class with the specified graph.
        /// </summary>
        /// <param name="graph">The graph to cache access to.</param>
        public CachedGraph(ISampledGraph graph)
        {
            Graph = graph;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="CachedGraph"/> class.
        /// </summary>
        public CachedGraph() { }

        /// <inheritdoc />
        public bool IsFinite => Graph.IsFinite;
        /// <inheritdoc />
        public bool IsDirected => Graph.IsDirected;

        /// <inheritdoc />
        public Guid BaseId => Graph.BaseId;

        /// <inheritdoc />
        public IMutableVertexAndEdgeSet<FreeformVertex, FreeformEdge> GetEntire()
        {
            return Graph.GetEntire();
        }

        /// <inheritdoc />
        public FreeformVertex GetProperties(Guid id)
        {
            // Try getting the cached value.
            if (Vertices.TryGetValue(id, out FreeformVertex cached)) return cached;

            // If not in cache, get the vertex from the original graph.
            FreeformVertex vertex = Graph.GetProperties(id);
            Vertices.Add(id, vertex);
            return vertex;
        }
        /// <inheritdoc />
        public IEnumerable<FreeformEdge> GetEdges(Guid id)
        {
            // Try getting the cached value.
            if (Edges.TryGetValue(id, out List<FreeformEdge> cached)) return cached;

            // If not in the cache, get the edges from the original graph.
            List<FreeformEdge> edges = Graph.GetEdges(id).ToList();
            Edges.Add(id, edges);
            return edges;
        }
    }
}