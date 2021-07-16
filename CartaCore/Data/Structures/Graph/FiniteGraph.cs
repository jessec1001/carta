using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MorseCode.ITask;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a graph that has finitely many vertices and edges.
    /// </summary>
    public class FiniteGraph : Graph,
        IEntireGraph,
        IDynamicInGraph<InOutVertex>,
        IDynamicOutGraph<InOutVertex>
    {
        /// <summary>
        /// Get or sets whether the graph is directed.
        /// </summary>
        /// <value><c>true</c> if the graph has directed edges; otherwise, <c>false</c>.</value>
        protected bool Directed { get; init; }

        /// <inheritdoc />
        public override GraphProperties GetProperties()
        {
            return new GraphProperties
            {
                Directed = Directed,
                Dynamic = true,
                Finite = true
            };
        }

        private Dictionary<Identity, Vertex> VertexSet { get; init; }
        private Dictionary<Identity, HashSet<Edge>> InEdgeSet { get; init; }
        private Dictionary<Identity, HashSet<Edge>> OutEdgeSet { get; init; }

        /// <summary>
        /// Initializes an instance of the <see cref="FiniteGraph"/> class with the specified identifier and properties.
        /// </summary>
        /// <param name="id">The graph identifier.</param>
        /// <param name="properties">The properties assigned to the graph.</param>
        /// <param name="directed">Whether the graph has directed or undirected edges.</param>
        public FiniteGraph(
            Identity id,
            IEnumerable<Property> properties,
            bool directed = true
        ) : base(id, properties)
        {
            Directed = directed;

            VertexSet = new Dictionary<Identity, Vertex>();
            InEdgeSet = new Dictionary<Identity, HashSet<Edge>>();
            OutEdgeSet = new Dictionary<Identity, HashSet<Edge>>();
        }
        /// <summary>
        /// Initializes an instance of the <see cref="FiniteGraph"/> class with the specified identifier.
        /// </summary>
        /// <param name="id">The graph identifier.</param>
        /// <param name="directed">Whether the graph has directed or undirected edges.</param>
        public FiniteGraph(
            Identity id,
            bool directed = true
        ) : base(id)
        {
            Directed = directed;

            VertexSet = new Dictionary<Identity, Vertex>();
            InEdgeSet = new Dictionary<Identity, HashSet<Edge>>();
            OutEdgeSet = new Dictionary<Identity, HashSet<Edge>>();
        }

        /// <summary>
        /// Adds a vertex to the graph.
        /// </summary>
        /// <param name="vertex">The vertex to add.</param>
        /// <returns><c>true</c> if the vertex was successfully added; otherwise, <c>false</c>.</returns>
        public bool AddVertex(IVertex vertex)
        {
            if (vertex is not Vertex vertexBase) return false;

            if (VertexSet.ContainsKey(vertex.Identifier))
                return false;
            else
            {
                VertexSet.Add(vertex.Identifier, vertexBase);
                if (vertex is IInVertex iVertex)
                    this.AddEdgeRange(iVertex.InEdges);
                if (vertex is IOutVertex oVertex)
                    this.AddEdgeRange(oVertex.OutEdges);
                return true;
            }
        }
        /// <summary>
        /// Adds a range of specified vertices to the graph.
        /// </summary>
        /// <param name="vertices">The vertices to add.</param>
        /// <returns>The count of successfully added vertices.</returns>
        public int AddVertexRange(IEnumerable<IVertex> vertices)
        {
            if (vertices is null) throw new ArgumentNullException(nameof(vertices));

            int added = 0;
            foreach (IVertex vertex in vertices)
                if (AddVertex(vertex)) added++;
            return added;
        }
        /// <summary>
        /// Removes a vertex from the graph.
        /// </summary>
        /// <param name="vertex">The vertex to remove.</param>
        /// <returns><c>true</c> if the vertex was successfully removed; otherwise, <c>false</c>.</returns>
        public bool RemoveVertex(IVertex vertex)
        {
            return VertexSet.Remove(vertex.Identifier);
        }
        /// <summary>
        /// Removes a range of specified vertices to the graph.
        /// </summary>
        /// <param name="vertices">The vertices to remove.</param>
        /// <returns>The count of successfully removed vertices.</returns>
        public int RemoveVertexRange(IEnumerable<IVertex> vertices)
        {
            if (vertices is null) throw new ArgumentNullException(nameof(vertices));

            int removed = 0;
            foreach (IVertex vertex in vertices)
                if (RemoveVertex(vertex)) removed++;
            return removed;
        }

        /// <summary>
        /// Adds an edge directed into a vertex.
        /// </summary>
        /// <param name="vertex">The vertex to add the edge to.</param>
        /// <param name="edge">The in-edge.</param>
        /// <returns><c>true</c> if the edge was added; otherwise, <c>false</c>.</returns>
        private bool AddInEdge(Identity vertex, Edge edge)
        {
            HashSet<Edge> edges;
            if (InEdgeSet.TryGetValue(vertex, out HashSet<Edge> existing))
                edges = existing;
            else
            {
                edges = new HashSet<Edge>();
                InEdgeSet.Add(vertex, edges);
            }
            return edges.Add(edge);
        }
        /// <summary>
        /// Adds an edge directed out of a vertex.
        /// </summary>
        /// <param name="vertex">The vertex to add the edge to.</param>
        /// <param name="edge">The out-edge.</param>
        /// <returns><c>true</c> if the edge was added; otherwise, <c>false</c>.</returns>
        private bool AddOutEdge(Identity vertex, Edge edge)
        {
            HashSet<Edge> edges;
            if (OutEdgeSet.TryGetValue(vertex, out HashSet<Edge> existing))
                edges = existing;
            else
            {
                edges = new HashSet<Edge>();
                OutEdgeSet.Add(vertex, edges);
            }
            return edges.Add(edge);
        }
        /// <summary>
        /// Removes an edge directed into a vertex.
        /// </summary>
        /// <param name="vertex">The vertex to remove the edge from.</param>
        /// <param name="edge">The in-edge.</param>
        /// <returns><c>true</c> if the edge was removed; otherwise, <c>false</c>.</returns>
        private bool RemoveInEdge(Identity vertex, Edge edge)
        {
            if (InEdgeSet.TryGetValue(vertex, out HashSet<Edge> edges))
                return edges.Remove(edge);
            return false;
        }
        /// <summary>
        /// Removes an edge directed out of a vertex.
        /// </summary>
        /// <param name="vertex">The vertex to remove the edge from.</param>
        /// <param name="edge">The out-edge.</param>
        /// <returns><c>true</c> if the edge was removed; otherwise, <c>false</c>.</returns>
        private bool RemoveOutEdge(Identity vertex, Edge edge)
        {
            if (OutEdgeSet.TryGetValue(vertex, out HashSet<Edge> edges))
                return edges.Remove(edge);
            return false;
        }

        /// <summary>
        /// Adds an edge to the graph.
        /// </summary>
        /// <param name="edge">
        /// The edge to add. This will be directed or undirected based on the corresponding graph property.
        /// </param>
        /// <returns><c>true</c> if the edge was successfully added; otherwise, <c>false</c>.</returns>
        public bool AddEdge(Edge edge)
        {
            if (Directed)
            {
                bool added = false;
                added = AddInEdge(edge.Target, edge) || added;
                added = AddOutEdge(edge.Source, edge) || added;
                return added;
            }
            else
            {
                bool added = false;
                added = AddInEdge(edge.Source, edge) || added;
                added = AddInEdge(edge.Target, edge) || added;
                added = AddOutEdge(edge.Source, edge) || added;
                added = AddOutEdge(edge.Target, edge) || added;
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
        public int AddEdgeRange(IEnumerable<Edge> edges)
        {
            if (edges is null) throw new ArgumentNullException(nameof(edges));

            int added = 0;
            foreach (Edge edge in edges)
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
        public bool RemoveEdge(Edge edge)
        {
            if (Directed)
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
        public int RemoveEdgeRange(IEnumerable<Edge> edges)
        {
            if (edges is null) throw new ArgumentNullException(nameof(edges));

            int removed = 0;
            foreach (Edge edge in edges)
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
        public async IAsyncEnumerable<IVertex> GetVertices()
        {
            foreach (Vertex vertex in VertexSet.Values)
            {
                InEdgeSet.TryGetValue(vertex.Identifier, out HashSet<Edge> inEdges);
                OutEdgeSet.TryGetValue(vertex.Identifier, out HashSet<Edge> outEdges);
                yield return await Task.FromResult
                (
                    new InOutVertex
                    (
                        vertex,
                        inEdges ?? new HashSet<Edge>(),
                        outEdges ?? new HashSet<Edge>()
                    )
                );
            }
        }

        /// <inheritdoc />
        public IEnumerable<Identity> Roots
        {
            get
            {
                return VertexSet.Values
                    .Where(vertex => !InEdgeSet.TryGetValue(vertex.Identifier, out HashSet<Edge> edges) || !edges.Any())
                    .Select(vertex => vertex.Identifier);
            }
        }

        /// <inheritdoc />
        public ITask<InOutVertex> GetVertex(Identity id)
        {
            if (!VertexSet.TryGetValue(id, out Vertex vertex))
                return Task.FromResult<InOutVertex>(null).AsITask();

            InEdgeSet.TryGetValue(id, out HashSet<Edge> inEdges);
            OutEdgeSet.TryGetValue(id, out HashSet<Edge> outEdges);
            InOutVertex inoutVertex = new InOutVertex
            (
                vertex,
                inEdges ?? new HashSet<Edge>(),
                outEdges ?? new HashSet<Edge>()
            );
            return Task.FromResult(inoutVertex).AsITask();
        }
        /// <inheritdoc />
        public async IAsyncEnumerable<InOutVertex> GetParentVertices(Identity id)
        {
            InOutVertex vertex = await GetVertex(id);
            foreach (Edge inEdge in vertex.InEdges)
                yield return await GetVertex(inEdge.Source);
        }
        /// <inheritdoc />
        public async IAsyncEnumerable<InOutVertex> GetChildVertices(Identity id)
        {
            InOutVertex vertex = await GetVertex(id);
            foreach (Edge outEdge in vertex.OutEdges)
                yield return await GetVertex(outEdge.Target);
        }
    }
}