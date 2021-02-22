using System;
using System.Collections.Generic;
using System.Linq;
using CartaCore.Integration.Hyperthought;
using QuikGraph;

namespace CartaCore.Data.Freeform
{
    /// <summary>
    /// Represents a mutable finite graph structure with no enforcement on requiring vertices before edges are included.
    /// </summary>
    public class FreeformFiniteGraph : FreeformGraph, IMutableVertexAndEdgeSet<FreeformVertex, FreeformEdge>
    {
        /// <summary>
        /// Whether the graph contains directed edges or undirected edges.
        /// </summary>
        private bool Directed;

        /// <summary>
        /// Contains the vertices that are included in the graph.
        /// </summary>
        private HashSet<FreeformVertex> VertexSet;
        /// <summary>
        /// Contains the edges that are included in the graph mapped by their source vertex.
        /// </summary>
        private Dictionary<FreeformVertex, HashSet<FreeformEdge>> EdgeMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="FreeformFiniteGraph"/> class.
        /// </summary>
        /// <param name="directed">Whether the graph contains directed edges or undirected edges.</param>
        public FreeformFiniteGraph(bool directed)
        {
            Directed = directed;

            VertexSet = new HashSet<FreeformVertex>();
            EdgeMap = new Dictionary<FreeformVertex, HashSet<FreeformEdge>>();
        }

        #region FreeformGraph
        /// <inheritdoc />
        public override bool IsDirected => Directed;
        /// <inheritdoc />
        public override bool AllowParallelEdges => true;

        /// <inheritdoc />
        public override bool IsVerticesEmpty => VertexSet.Any();
        /// <inheritdoc />
        public override bool IsEdgesEmpty => EdgeMap.Any();

        /// <inheritdoc />
        public override int VertexCount => VertexSet.Count;
        /// <inheritdoc />
        public override int EdgeCount => EdgeMap.Sum(pair => pair.Value.Count);

        /// <inheritdoc />
        public override IEnumerable<FreeformVertex> Vertices => VertexSet;
        /// <inheritdoc />
        public override IEnumerable<FreeformEdge> Edges => EdgeMap.SelectMany(pair => pair.Value);

        /// <inheritdoc />
        public override bool ContainsVertex(FreeformVertex vertex)
        {
            return VertexSet.Contains(vertex);
        }
        /// <inheritdoc />
        public override bool ContainsEdge(FreeformEdge edge)
        {
            bool contains;
            contains = EdgeMap.TryGetValue(edge.Source, out HashSet<FreeformEdge> outEdges);
            contains = contains && outEdges.Contains(edge);
            return contains;
        }
        #endregion

        #region IMutableVertexAndEdgeSet
        /// <inheritdoc />
        public event VertexAction<FreeformVertex> VertexAdded;
        /// <inheritdoc />
        public event VertexAction<FreeformVertex> VertexRemoved;
        /// <inheritdoc />
        public event EdgeAction<FreeformVertex, FreeformEdge> EdgeAdded;
        /// <inheritdoc />
        public event EdgeAction<FreeformVertex, FreeformEdge> EdgeRemoved;

        /// <summary>
        /// Called on each added vertex.
        /// </summary>
        /// <param name="vertex">The added vertex.</param>
        protected void OnVertexAdded(FreeformVertex vertex) => VertexAdded?.Invoke(vertex);
        /// <summary>
        /// Called on each removed vertex.
        /// </summary>
        /// <param name="vertex">The removed vertex.</param>
        protected void OnVertexRemoved(FreeformVertex vertex) => VertexRemoved?.Invoke(vertex);
        /// <summary>
        /// Called on each added edge.
        /// </summary>
        /// <param name="edge">The added edge.</param>
        protected void OnEdgeAdded(FreeformEdge edge) => EdgeAdded?.Invoke(edge);
        /// <summary>
        /// Called on each removed edge.
        /// </summary>
        /// <param name="edge">The removed edge.</param>
        protected void OnEdgeRemoved(FreeformEdge edge) => EdgeRemoved?.Invoke(edge);

        /// <inheritdoc />
        public bool AddVertex(FreeformVertex vertex)
        {
            // We cannot add a null vertex.
            if (vertex is null) return false;

            // If we already have the vertex, don't add it again.
            if (VertexSet.Contains(vertex)) return false;

            // Add the vertex.
            bool success = VertexSet.Add(vertex);
            OnVertexAdded(vertex);
            return success;
        }
        /// <inheritdoc />
        public int AddVertexRange(IEnumerable<FreeformVertex> vertices)
        {
            // We cannot have a null enumerable of vertices.
            if (vertices is null)
                throw new ArgumentNullException(nameof(vertices));

            // Add each of the vertices one at a time keeping track of how many were added.
            int added = 0;
            foreach (FreeformVertex vertex in vertices)
                if (AddVertex(vertex)) added++;
            return added;
        }
        /// <inheritdoc />
        public bool RemoveVertex(FreeformVertex vertex)
        {
            // Try to remove vertex.
            bool success = VertexSet.Remove(vertex);
            if (success) OnVertexRemoved(vertex);
            return success;
        }
        /// <inheritdoc />
        public int RemoveVertexIf(VertexPredicate<FreeformVertex> predicate)
        {
            // We cannot have a null predicate.
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            // Remove each of the vertices keeping track of how many were removed.
            int removed = 0;
            foreach (FreeformVertex vertex in VertexSet.Where(vertex => predicate(vertex)))
                if (RemoveVertex(vertex)) removed++;
            return removed;
        }

        /// <inheritdoc />
        public bool AddEdge(FreeformEdge edge)
        {
            // We cannot add a null edge.
            if (edge is null) return false;

            // We need to setup a new out-edges set if it does not already exist.
            EdgeMap.TryGetValue(edge.Source, out HashSet<FreeformEdge> outEdges);
            if (outEdges is null)
            {
                outEdges = new HashSet<FreeformEdge>();
                EdgeMap.Add(edge.Source, outEdges);
            }

            // Add the edge.
            bool success = outEdges.Add(edge);
            OnEdgeAdded(edge);
            return success;
        }
        /// <inheritdoc />
        public int AddEdgeRange(IEnumerable<FreeformEdge> edges)
        {
            // We cannot have a null enumerable of edges.
            if (edges is null)
                throw new ArgumentNullException(nameof(edges));

            // Add each of the edges one at a time keeping track of how many were added.
            int added = 0;
            foreach (FreeformEdge edge in edges)
                if (AddEdge(edge)) added++;
            return added;
        }
        /// <inheritdoc />
        public bool RemoveEdge(FreeformEdge edge)
        {
            // Try to remove edge.
            bool success;
            success = EdgeMap.TryGetValue(edge.Source, out HashSet<FreeformEdge> outEdges);
            success = success && outEdges.Remove(edge);
            if (success) OnEdgeRemoved(edge);
            return success;
        }
        /// <inheritdoc />
        public int RemoveEdgeIf(EdgePredicate<FreeformVertex, FreeformEdge> predicate)
        {
            // We cannot have a null predicate.
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            // Remove each of the edges keeping track of how many were removed.
            int removed = 0;
            foreach (FreeformEdge edge in EdgeMap.SelectMany(pair => pair.Value.Where(edge => predicate(edge))))
                if (RemoveEdge(edge)) removed++;
            return removed;
        }

        /// <inheritdoc />
        public bool AddVerticesAndEdge(FreeformEdge edge)
        {
            // We cannot add a null edge.
            if (edge is null) return false;

            // Add the endpoint vertices and the edge.
            AddVertex(edge.Source);
            AddVertex(edge.Target);
            return AddEdge(edge);
        }
        /// <inheritdoc />
        public int AddVerticesAndEdgeRange(IEnumerable<FreeformEdge> edges)
        {
            // We cannot have a null enumerable of edges.
            if (edges is null)
                throw new ArgumentNullException(nameof(edges));

            // Add the endpoint vertices and the edges keeping track of how many were added.
            int added = 0;
            foreach (FreeformEdge edge in edges)
                if (AddVerticesAndEdge(edge)) added++;
            return added;
        }
        /// <inheritdoc />
        public void Clear()
        {
            // Notify about all the removals.
            foreach (FreeformVertex vertex in Vertices) OnVertexRemoved(vertex);
            foreach (FreeformEdge edge in Edges) OnEdgeRemoved(edge);

            // Clear the data.
            VertexSet.Clear();
            EdgeMap.Clear();
        }
        #endregion

        /// <summary>
        /// Creates a subgraph of an existing graph with the specified included identifiers.
        /// </summary>
        /// <param name="parent">The parent graph of the new subgraph.</param>
        /// <param name="includedIds">
        /// The vertex identifiers to include in the subgraph. If set to <c>null</c>, all of the vertices will be
        /// included in the subgraph.
        /// </param>
        /// <param name="includeEdges">Whether or not to include the edges attached to the included identifiers.</param>
        /// <returns>The created subgraph.</returns>
        public static FreeformFiniteGraph CreateSubgraph(
            FreeformGraph parent,
            IEnumerable<FreeformIdentity> includedIds,
            bool includeEdges = true)
        {
            // Create the graph.
            FreeformFiniteGraph graph = new FreeformFiniteGraph(parent.IsDirected);

            // If the included identifiers is null, include all vertices.
            // Otherwise, include only the vertices that are specified by identifier.
            if (includedIds is null)
            {
                graph.AddVertexRange(parent.Vertices);
                if (includeEdges)
                    graph.AddEdgeRange(parent.Edges);
            }
            else
            {
                HashSet<FreeformIdentity> includedSet = new HashSet<FreeformIdentity>(includedIds);
                graph.AddVertexRange(parent.Vertices.Where(vertex => includedSet.Contains(vertex.Identifier)));
                if (includeEdges)
                    graph.AddEdgeRange(parent.Edges.Where(edge => includedSet.Contains(edge.Source.Identifier)));
            }

            return graph;
        }
        /// <summary>
        /// Creates a subgraph of an existing dynamic graph with the specified included identifiers.
        /// </summary>
        /// <param name="parent">The dynamic parent graph of the new subgraph.</param>
        /// <param name="includedIds">
        /// The vertex identifiers to include in the subgraph. If set to <c>null</c>, all of the vertices will be
        /// included in the subgraph.
        /// </param>
        /// <param name="includeEdges">Whether or not to include the edges attached to the included identifiers.</param>
        /// <returns>The created subgraph.</returns>
        public static FreeformFiniteGraph CreateSubgraph(
            FreeformDynamicGraph parent,
            IEnumerable<FreeformIdentity> includedIds,
            bool includeEdges = true)
        {
            // Create the graph.
            FreeformFiniteGraph graph = new FreeformFiniteGraph(parent.IsDirected);

            // If the included identifiers is null, include all vertices.
            // Otherwise, include only the vertices that are specified by identifier.
            if (includedIds is null)
            {
                graph.AddVertexRange(parent.Vertices);
                if (includeEdges)
                    graph.AddEdgeRange(parent.Edges);
            }
            else
            {
                // If we need to include the edges, we perform the combined vertex and edge retrieval.
                if (includeEdges)
                {
                    foreach (FreeformIdentity id in includedIds)
                    {
                        (FreeformVertex vertex, IEnumerable<FreeformEdge> edges) = parent.GetVertexWithEdges(id);
                        graph.AddVertex(vertex);
                        if (!(edges is null)) graph.AddEdgeRange(edges);
                    }
                }
                else
                {
                    if (parent is HyperthoughtWorkflowGraph htParent)
                    {
                        graph.AddVertexRange(htParent.GetVertices(includedIds));
                    }
                    else
                    {
                        graph.AddVertexRange(includedIds.Select(id => parent.GetVertex(id)));
                    }
                }
            }

            return graph;
        }

        /// <summary>
        /// Creates a subgraph of an existing dynamic graph with the specified included parent identifiers. The children
        /// of these parent identifiers are included in the graph instead of the parents themselves.
        /// </summary>
        /// <param name="parent">The dynamic parent graph of the new subgraph.</param>
        /// <param name="includedIds">
        /// The parent vertex identifiers to include in the subgraph. If set to <c>null</c>, all of the vertices will be
        /// included in the subgraph.
        /// </param>
        /// <param name="includeEdges">Whether or not to include the edges attached to the included identifiers.</param>
        /// <returns>The created subgraph.</returns>
        public static FreeformFiniteGraph CreateChildSubgraph(
            FreeformDynamicGraph parent,
            IEnumerable<FreeformIdentity> includedIds,
            bool includeEdges = true)
        {
            // Create the graph.
            FreeformFiniteGraph graph = new FreeformFiniteGraph(parent.IsDirected);

            // If the included identifiers is null, include all vertices.
            // Otherwise, include only the vertices that are specified by identifier.
            if (includedIds is null)
            {
                graph.AddVertexRange(parent.Vertices);
                if (includeEdges)
                    graph.AddEdgeRange(parent.Edges);
            }
            else
            {
                // If we need to include the edges, we perform the combined vertex and edge retrieval.
                if (includeEdges)
                {
                    foreach (FreeformIdentity id in includedIds)
                    {
                        (IEnumerable<FreeformVertex> vertices, IEnumerable<FreeformEdge> edges) = parent.GetChildVerticesWithEdges(id);
                        if (!(vertices is null)) graph.AddVertexRange(vertices);
                        if (!(edges is null)) graph.AddEdgeRange(edges);
                    }
                }
                else
                    graph.AddVertexRange(includedIds.SelectMany(id => parent.GetChildVertices(id) ?? Enumerable.Empty<FreeformVertex>()));
            }

            return graph;
        }
    }
}