using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CartaCore.Graphs.Components;
using MorseCode.ITask;

namespace CartaCore.Graphs
{
    /// <summary>
    /// Represents a graph that has finitely many vertices and edges.
    /// </summary>
    /// <typeparam name="TVertex">The type of vertex.</typeparam>
    /// <typeparam name="TEdge">The type of edge.</typeparam>
    public class MemoryGraph<TVertex, TEdge> : Graph,
        IRootedComponent,
        IEnumerableComponent<TVertex, TEdge>,
        IDynamicLocalComponent<TVertex, TEdge>,
        IDynamicInComponent<TVertex, TEdge>,
        IDynamicOutComponent<TVertex, TEdge>
        where TVertex : IVertex<TEdge>
        where TEdge : IEdge
    {
        /// <inheritdoc />
        public override GraphAttributes Attributes => new()
        {
            Dynamic = true,
            Finite = true
        };

        private Dictionary<string, TVertex> VertexSet { get; init; }
        private Dictionary<string, HashSet<TEdge>> InEdgeSet { get; init; }
        private Dictionary<string, HashSet<TEdge>> OutEdgeSet { get; init; }

        /// <summary>
        /// Initializes an instance of the <see cref="MemoryGraph{TVertex, TEdge}"/> class with the specified identifier
        /// and properties.
        /// </summary>
        /// <param name="id">The graph identifier.</param>
        /// <param name="properties">The properties assigned to the graph.</param>
        public MemoryGraph(
            string id,
            IDictionary<string, IProperty> properties
        ) : base(id, properties)
        {
            VertexSet = new Dictionary<string, TVertex>();
            InEdgeSet = new Dictionary<string, HashSet<TEdge>>();
            OutEdgeSet = new Dictionary<string, HashSet<TEdge>>();
        }
        /// <summary>
        /// Initializes an instance of the <see cref="MemoryGraph{TVertex, TEdge}"/> class with the specified
        /// identifier.
        /// </summary>
        /// <param name="id">The graph identifier.</param>
        public MemoryGraph(
            string id
        ) : base(id, new Dictionary<string, IProperty>()) { }

        /// <summary>
        /// Adds a vertex to the graph.
        /// </summary>
        /// <param name="vertex">The vertex to add.</param>
        /// <returns><c>true</c> if the vertex was successfully added; otherwise, <c>false</c>.</returns>
        public bool AddVertex(TVertex vertex)
        {
            if (VertexSet.ContainsKey(vertex.Id))
                return false;
            else
            {
                VertexSet.Add(vertex.Id, vertex);
                AddEdgeRange(vertex.Edges);
                return true;
            }
        }
        /// <summary>
        /// Adds a range of specified vertices to the graph.
        /// </summary>
        /// <param name="vertices">The vertices to add.</param>
        /// <returns>The count of successfully added vertices.</returns>
        public int AddVertexRange(IEnumerable<TVertex> vertices)
        {
            if (vertices is null) throw new ArgumentNullException(nameof(vertices));

            int added = 0;
            foreach (TVertex vertex in vertices)
                if (AddVertex(vertex)) added++;
            return added;
        }
        /// <summary>
        /// Removes a vertex from the graph.
        /// </summary>
        /// <param name="vertex">The vertex to remove.</param>
        /// <returns><c>true</c> if the vertex was successfully removed; otherwise, <c>false</c>.</returns>
        public bool RemoveVertex(TVertex vertex)
        {
            return VertexSet.Remove(vertex.Id);
        }
        /// <summary>
        /// Removes a vertex from the graph specified by an identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns><c>true</c> if the vertex was successfully removed; otherwise, <c>false</c>.</returns>
        public bool RemoveVertex(string id)
        {
            return VertexSet.Remove(id);
        }
        /// <summary>
        /// Removes a range of specified vertices from the graph.
        /// </summary>
        /// <param name="vertices">The vertices to remove.</param>
        /// <returns>The count of successfully removed vertices.</returns>
        public int RemoveVertexRange(IEnumerable<TVertex> vertices)
        {
            if (vertices is null) throw new ArgumentNullException(nameof(vertices));

            int removed = 0;
            foreach (TVertex vertex in vertices)
                if (RemoveVertex(vertex)) removed++;
            return removed;
        }
        /// <summary>
        /// Removes a range of vertices specified by identifiers from the graph.
        /// </summary>
        /// <param name="ids">The identifiers.</param>
        /// <returns>The count of successfully removed vertices.</returns>
        public int RemoveVertexRange(IEnumerable<string> ids)
        {
            if (ids is null) throw new ArgumentNullException(nameof(ids));

            int removed = 0;
            foreach (string id in ids)
                if (RemoveVertex(id)) removed++;
            return removed;
        }

        /// <summary>
        /// Adds an edge directed into a vertex.
        /// </summary>
        /// <param name="target">The vertex to add the edge to.</param>
        /// <param name="edge">The in-edge.</param>
        /// <returns><c>true</c> if the edge was added; otherwise, <c>false</c>.</returns>
        private bool AddInEdge(string target, TEdge edge)
        {
            HashSet<TEdge> edges;
            if (InEdgeSet.TryGetValue(target, out HashSet<TEdge> existing))
                edges = existing;
            else
            {
                edges = new HashSet<TEdge>();
                InEdgeSet.Add(target, edges);
            }
            return edges.Add(edge);
        }
        /// <summary>
        /// Adds an edge directed out of a vertex.
        /// </summary>
        /// <param name="source">The vertex to add the edge to.</param>
        /// <param name="edge">The out-edge.</param>
        /// <returns><c>true</c> if the edge was added; otherwise, <c>false</c>.</returns>
        private bool AddOutEdge(string source, TEdge edge)
        {
            HashSet<TEdge> edges;
            if (OutEdgeSet.TryGetValue(source, out HashSet<TEdge> existing))
                edges = existing;
            else
            {
                edges = new HashSet<TEdge>();
                OutEdgeSet.Add(source, edges);
            }
            return edges.Add(edge);
        }
        /// <summary>
        /// Removes an edge directed into a vertex.
        /// </summary>
        /// <param name="target">The vertex to remove the edge from.</param>
        /// <param name="edge">The in-edge.</param>
        /// <returns><c>true</c> if the edge was removed; otherwise, <c>false</c>.</returns>
        private bool RemoveInEdge(string target, TEdge edge)
        {
            if (InEdgeSet.TryGetValue(target, out HashSet<TEdge> edges))
                return edges.Remove(edge);
            return false;
        }
        /// <summary>
        /// Removes an edge directed out of a vertex.
        /// </summary>
        /// <param name="source">The vertex to remove the edge from.</param>
        /// <param name="edge">The out-edge.</param>
        /// <returns><c>true</c> if the edge was removed; otherwise, <c>false</c>.</returns>
        private bool RemoveOutEdge(string source, TEdge edge)
        {
            if (OutEdgeSet.TryGetValue(source, out HashSet<TEdge> edges))
                return edges.Remove(edge);
            return false;
        }

        /// <summary>
        /// Adds an edge to the graph.
        /// </summary>
        /// <param name="edge">
        /// The edge to add. This will be directed or undirected based on the edge property.
        /// </param>
        /// <returns><c>true</c> if the edge was successfully added; otherwise, <c>false</c>.</returns>
        public bool AddEdge(TEdge edge)
        {
            if (edge.Directed)
            {
                bool added = false;
                added = AddInEdge(edge.Target, edge) || added;
                added = AddOutEdge(edge.Source, edge) || added;
                return added;
            }
            else
            {
                bool added = false;
                added = AddInEdge(edge.Target, edge) || added;
                added = AddOutEdge(edge.Target, edge) || added;
                added = AddInEdge(edge.Source, edge) || added;
                added = AddOutEdge(edge.Source, edge) || added;
                return added;
            }
        }
        /// <summary>
        /// Adds a range of specified edges to the graph.
        /// </summary>
        /// <param name="edges">
        /// The edges to add. These will be directed or undirected based on the corresponding graph property.
        /// </param>
        /// <returns>The count of successfully added edges.</returns>
        public int AddEdgeRange(IEnumerable<TEdge> edges)
        {
            if (edges is null) throw new ArgumentNullException(nameof(edges));

            int added = 0;
            foreach (TEdge edge in edges)
                if (AddEdge(edge)) added++;
            return added;
        }
        /// <summary>
        /// Removes an edge from the graph.
        /// </summary>
        /// <param name="edge">
        /// The edge to remove.
        /// </param>
        /// <returns><c>true</c> if the edge was successfully removed; otherwise, <c>false</c>.</returns>
        public bool RemoveEdge(TEdge edge)
        {
            if (edge.Directed)
            {
                bool removed = false;
                removed = RemoveInEdge(edge.Target, edge) || removed;
                removed = RemoveOutEdge(edge.Source, edge) || removed;
                return removed;
            }
            else
            {
                bool removed = false;
                removed = RemoveInEdge(edge.Source, edge) || removed;
                removed = RemoveInEdge(edge.Target, edge) || removed;
                removed = RemoveOutEdge(edge.Source, edge) || removed;
                removed = RemoveOutEdge(edge.Target, edge) || removed;
                return removed;
            }
        }
        /// <summary>
        /// removes a range of specified edges to the graph.
        /// </summary>
        /// <param name="edges">
        /// The edges to remove.
        /// </param>
        /// <returns>The count of successfully removed edges.</returns>
        public int RemoveEdgeRange(IEnumerable<TEdge> edges)
        {
            if (edges is null) throw new ArgumentNullException(nameof(edges));

            int removed = 0;
            foreach (TEdge edge in edges)
                if (RemoveEdge(edge)) removed++;
            return removed;
        }

        /// <summary>
        /// Clears all of the vertices and edges from the graph.
        /// </summary>
        public void Clear()
        {
            VertexSet.Clear();
            InEdgeSet.Clear();
            OutEdgeSet.Clear();
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<TVertex> GetVertices()
        {
            // Get all of the vertices in the vertex set.
            foreach (string id in VertexSet.Keys)
            {
                if (VertexSet.TryGetValue(id, out TVertex vertex))
                    yield return await Task.FromResult(vertex);
            }
        }
        /// <inheritdoc />
        public async IAsyncEnumerable<TEdge> GetEdges()
        {
            // Get all of the vertex identifiers in the edge set.
            // Notice that this may differ from the vertex identifiers in the vertex set because vertices are allowed to
            // not have a value assigned to them but still have edges.
            foreach (string id in OutEdgeSet.Keys)
            {
                // Get the out-edges for the vertex.
                // Iterate over them and return.
                if (OutEdgeSet.TryGetValue(id, out HashSet<TEdge> outEdges))
                    foreach (TEdge edge in outEdges) yield return await Task.FromResult(edge);
            }
        }

        /// <inheritdoc />
        public IAsyncEnumerable<string> Roots()
        {
            return VertexSet.Values
                .Where(vertex => !InEdgeSet.TryGetValue(vertex.Id, out HashSet<TEdge> edges) || !edges.Any())
                .Select(vertex => vertex.Id)
                .ToAsyncEnumerable();
        }

        /// <inheritdoc />
        public ITask<TVertex> GetVertex(string id)
        {
            // Get the vertex.
            if (VertexSet.TryGetValue(id, out TVertex vertex))
                return Task.FromResult(vertex).AsITask();
            return Task.FromResult(default(TVertex)).AsITask();
        }
        /// <inheritdoc />
        public async IAsyncEnumerable<TVertex> GetParentVertices(string id)
        {
            InEdgeSet.TryGetValue(id, out HashSet<TEdge> inEdges);
            if (inEdges is not null)
            {
                foreach (TEdge edge in inEdges)
                    yield return await GetVertex(edge.Source);
            }
        }
        /// <inheritdoc />
        public async IAsyncEnumerable<TVertex> GetChildVertices(string id)
        {
            OutEdgeSet.TryGetValue(id, out HashSet<TEdge> outEdges);
            if (outEdges is not null)
            {
                foreach (TEdge edge in outEdges)
                    yield return await GetVertex(edge.Target);
            }
        }
    }

    /// <summary>
    /// Represents a graph that has finitely many vertices and edges.
    /// </summary>
    public class MemoryGraph : MemoryGraph<Vertex, Edge>
    {
        /// <summary>
        /// Initializes an instance of the <see cref="MemoryGraph"/> class with the specified identifier
        /// and properties.
        /// </summary>
        /// <param name="id">The graph identifier.</param>
        /// <param name="properties">The properties assigned to the graph.</param>
        public MemoryGraph(
            string id,
            IDictionary<string, IProperty> properties
        ) : base(id, properties) { }
        /// <summary>
        /// Initializes an instance of the <see cref="MemoryGraph"/> class with the specified
        /// identifier.
        /// </summary>
        /// <param name="id">The graph identifier.</param>
        public MemoryGraph(
            string id
        ) : base(id) { }
    }
}