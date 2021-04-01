using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a graph that has finitely many vertices and edges that can be enumerated over.
    /// </summary>
    public class FiniteGraph : Graph, IEntireGraph
    {
        /// <summary>
        /// Gets whether the graph contains directed edges or undirected edges.
        /// </summary>
        /// <value><c>true</c> if the graph edges are directed; otherwise, <c>false</c>.</value>
        private bool Directed { get; init; }
        /// <summary>
        /// Gets whether the graph is dynamically loaded or not.
        /// </summary>
        /// <value><c>true</c> if the graph is dynamic; otherwise, <c>false</c>.</value>
        private bool Dynamic { get; init; }
        /// <summary>
        /// Gets the set of vertices.
        /// </summary>
        /// <value>The set of vertices.</value>
        private HashSet<IVertex> VertexSet { get; init; }
        /// <summary>
        /// Gets the set of edges.
        /// </summary>
        /// <value>The set of edges.</value>
        private HashSet<Edge> EdgeSet { get; init; }

        public IGraph UnderlyingGraph { get; set; }

        /// <inheritdoc />
        public override bool IsDirected => Directed;
        /// <inheritdoc />
        public override bool IsDynamic => Dynamic;
        /// <inheritdoc />
        public override bool IsFinite => true;

        /// <inheritdoc />
        public IAsyncEnumerable<IVertex> Vertices => VertexSet.ToAsyncEnumerable();
        /// <inheritdoc />
        public IAsyncEnumerable<Edge> Edges => EdgeSet.ToAsyncEnumerable();

        /// <summary>
        /// Initializes an instance of the <see cref="FiniteGraph"/> class with the specified identifier, properties,
        /// and edge directionality.
        /// </summary>
        /// <param name="id">The graph identifier.</param>
        /// <param name="properties">The properties assigned to the graph.</param>
        /// <param name="directed"><c>true</c> if the edges are directed; otherwise, <c>false</c>.</param>
        /// <param name="dynamic"><c>true</c> if the graph is dynamic; otherwise, <c>false</c>.</param>
        public FiniteGraph(
            Identity id, IEnumerable<Property> properties,
            bool directed = true,
            bool dynamic = true
        ) : base(id, properties)
        {
            Directed = directed;
            Dynamic = dynamic;
            VertexSet = new HashSet<IVertex>();
            EdgeSet = new HashSet<Edge>();
        }
        /// <summary>
        /// Initializes an instance of the <see cref="FiniteGraph"/> class with the specified identifier and edge
        /// directionality.
        /// </summary>
        /// <param name="id">The graph identifier.</param>
        /// <param name="directed"><c>true</c> if the edges are directed; otherwise, <c>false</c>.</param>
        /// <param name="dynamic"><c>true</c> if the graph is dynamic; otherwise, <c>false</c>.</param>
        public FiniteGraph(Identity id, bool directed = true, bool dynamic = true)
            : base(id)
        {
            Directed = directed;
            Dynamic = dynamic;
            VertexSet = new HashSet<IVertex>();
            EdgeSet = new HashSet<Edge>();
        }

        /// <summary>
        /// Adds a vertex to the graph.
        /// </summary>
        /// <param name="vertex">The vertex to add.</param>
        /// <returns><c>true</c> if the vertex was successfully added; otherwise, <c>false</c>.</returns>
        public bool AddVertex(IVertex vertex)
        {
            if (vertex is null) return false;
            return VertexSet.Add(vertex);
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
            if (vertex is null) return false;
            return VertexSet.Remove(vertex);
        }
        /// <summary>
        /// Removes any vertex from the graph satisfying a specified predicate condition.
        /// </summary>
        /// <param name="predicate">The condition on which to remove vertices.</param>
        /// <returns>The count of successfully removed vertices.</returns>
        public int RemoveVertexOn(Predicate<IVertex> predicate)
        {
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            int removed = 0;
            foreach (IVertex vertex in VertexSet)
                if (predicate(vertex) && RemoveVertex(vertex)) removed++;
            return removed;
        }

        /// <summary>
        /// Adds an edge to the graph.
        /// </summary>
        /// <param name="edge">The edge to add.</param>
        /// <returns><c>true</c> if the edge was successfully added; otherwise, <c>false</c>.</returns>
        public bool AddEdge(Edge edge)
        {
            if (edge is null) return false;
            return EdgeSet.Add(edge);
        }
        /// <summary>
        /// Adds a range of specified edges to the graph.
        /// </summary>
        /// <param name="edges">The edges to add.</param>
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
        /// <param name="edge">The edge to remove.</param>
        /// <returns><c>true</c> if the edge was successfully removed; otherwise, <c>false</c>.</returns>
        public bool RemoveEdge(Edge edge)
        {
            if (edge is null) return false;
            return EdgeSet.Remove(edge);
        }
        /// <summary>
        /// Removes any edge from the graph satisfying a specified predicate condition.
        /// </summary>
        /// <param name="predicate">The condition on which to remove edges.</param>
        /// <returns>The count of successfully removed edges.</returns>
        public int RemoveEdgeOn(Predicate<Edge> predicate)
        {
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            int removed = 0;
            foreach (Edge edge in EdgeSet)
                if (predicate(edge) && RemoveEdge(edge)) removed++;
            return removed;
        }

        /// <summary>
        /// Clears all of the vertices and edges from the graph.
        /// </summary>
        public void Clear()
        {
            VertexSet.Clear();
            EdgeSet.Clear();
        }

        /// <summary>
        /// Creates a subgraph of a given graph with vertices specified by identifiers included.
        /// </summary>
        /// <param name="graph">The base graph.</param>
        /// <param name="ids">The identifiers specifying which vertices to include.</param>
        /// <returns>A finite subgraph of the base graph.</returns>
        public static async Task<FiniteGraph> CreateSubgraph(IDynamicGraph<Vertex> graph, IEnumerable<Identity> ids)
        {
            // Copy over the settings from the base graph into a new finite graph.
            FiniteGraph subgraph;
            if (graph is Graph graphBase)
                subgraph = new FiniteGraph(graphBase.Identifier, graphBase.Properties, graph.IsDirected)
                {
                    Label = graphBase.Label,
                    Description = graphBase.Description
                };
            else subgraph = new FiniteGraph(null, graph.IsDirected);
            subgraph.UnderlyingGraph = graph;

            // Get the vertices from the graph.
            await foreach (Vertex vertex in graph.GetVertices(ids))
                subgraph.AddVertex(vertex);

            return subgraph;
        }
        /// <summary>
        /// Creates a subgraph of a given graph with vertices and out-edges specified by identifiers included.
        /// </summary>
        /// <param name="graph">The base graph.</param>
        /// <param name="ids">The identifiers specifying which vertices and out-edges to include.</param>
        /// <returns>A finite subgraph of the base graph.</returns>
        public static async Task<FiniteGraph> CreateSubgraph(IDynamicOutGraph<IOutVertex> graph, IEnumerable<Identity> ids)
        {
            // Copy over the settings from the base graph into a new finite graph.
            FiniteGraph subgraph;
            if (graph is Graph graphBase)
                subgraph = new FiniteGraph(graphBase.Identifier, graphBase.Properties, graph.IsDirected)
                {
                    Label = graphBase.Label,
                    Description = graphBase.Description
                };
            else subgraph = new FiniteGraph(null, graph.IsDirected);
            subgraph.UnderlyingGraph = graph;

            // Get the vertices from the graph.
            await foreach (IOutVertex vertex in graph.GetVertices(ids))
            {
                subgraph.AddVertex(vertex);
                subgraph.AddEdgeRange(vertex.OutEdges);
            }

            return subgraph;
        }

        /// <summary>
        /// Creates a child subgraph of a given graph with the children vertices specified by identifiers included.
        /// </summary>
        /// <param name="graph">The base graph.</param>
        /// <param name="ids">The identifiers specifying which vertices' children to include.</param>
        /// <returns>A finite child subgraph of the base graph.</returns>
        public static async Task<FiniteGraph> CreateChildSubgraph(IDynamicOutGraph<IOutVertex> graph, IEnumerable<Identity> ids)
        {
            // Copy over the settings from the base graph into a new finite graph.
            FiniteGraph subgraph;
            if (graph is Graph graphBase)
                subgraph = new FiniteGraph(graphBase.Identifier, graphBase.Properties, graph.IsDirected)
                {
                    Label = graphBase.Label,
                    Description = graphBase.Description
                };
            else subgraph = new FiniteGraph(null, graph.IsDirected);
            subgraph.UnderlyingGraph = graph;

            // Get the child vertices from the graph.
            foreach (Identity id in ids)
            {
                await foreach (IOutVertex vertex in graph.GetChildVertices(id))
                {
                    subgraph.AddVertex(vertex);
                    subgraph.AddEdgeRange(vertex.OutEdges);
                }
            }

            return subgraph;
        }
    }
}