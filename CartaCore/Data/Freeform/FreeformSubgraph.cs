using System;
using System.Collections.Generic;
using System.Linq;

namespace CartaCore.Data.Freeform
{
    /// <summary>
    /// Represents a subgraph of another graph structure with a set of specified contained vertices and edges.
    /// </summary>
    public class FreeformSubgraph : FreeformGraph
    {
        /// <summary>
        /// Gets or sets the parent graph.
        /// </summary>
        /// <value>The parent of this subgraph.</value>
        public FreeformGraph Parent { get; protected set; }
        /// <summary>
        /// Gets or sets the dynamic version of the parent graph.
        /// </summary>
        /// <value>The dynamic parent of this subgraph. Set to <c>null</c> if the parent is not dynamic.</value>
        protected FreeformDynamicGraph DynamicParent { get; set; }

        /// <summary>
        /// Gets or sets the vertex identifiers contained in this subgraph. 
        /// </summary>
        /// <value>The contained vertex identifiers.</value>
        public ISet<FreeformIdentity> FilteredIds { get; protected set; }

        /// <summary>
        /// Gets or sets the computed vertices.
        /// </summary>
        /// <value>
        /// If <see cref="Computed"/> is <c>true</c>, this contains the computed vertices. Otherwise, this is set to
        /// <c>null</c>.
        /// </value>
        private ISet<FreeformVertex> ComputedVertices { get; set; }
        /// <summary>
        /// Gets or sets the computed edges.
        /// </summary>
        /// <value>
        /// If <see cref="Computed"/> is <c>true</c>, this contains the computed edges. Otherwise, this is set to
        /// <c>null</c>.
        /// </value>
        private ISet<FreeformEdge> ComputedEdges { get; set; }

        /// <summary>
        /// Gets or sets whether to get the children or parent vertices and edges.
        /// </summary>
        /// <value>
        /// <c>true</c> if the children should be contained instead of its parents; <c>false</c> if only the filtered
        /// identifiers should be included.
        /// </value>
        public bool Children { get; protected set; }
        /// <summary>
        /// Gets or sets whther the subgraph is computed.
        /// </summary>
        /// <value>
        /// <c>true</c> if the vertices and edges are computed on initialization which may lead to performance
        /// improvements at the expense of memory; <c>false</c> if the vertices and edges are computed on request.
        /// </value>
        public bool Computed { get; protected set; }

        /// <summary>
        /// Initializes an instance of the <see cref="FreeformSubgraph"/> object of a parent <see cref="FreeformGraph"/>
        /// with the specified contained vertices and edges.
        /// </summary>
        /// <remarks>
        /// If either the contained vertices or contained edges are set to <c>null</c>, then all of the respective
        /// object are contained.
        /// </remarks>
        /// <param name="parent">The parent graph.</param>
        /// <param name="ids">The contained vertices. May be <c>null</c>.</param>
        /// <param name="children">
        /// Whether or not the children of the filtered identifiers should be contained instead of the vertex itself.
        /// </param>
        /// <param name="computed">
        /// Whether or not to compute the subgraph on initialization. This may improve performance but requires memory.
        /// </param>
        public FreeformSubgraph(FreeformGraph parent, IEnumerable<FreeformIdentity> ids, bool children = false, bool computed = false)
        {
            // Get the parent and its possible dynamic version.
            Parent = parent;
            DynamicParent = parent as FreeformDynamicGraph;

            // Set the contained vertices.
            // A value of null for either means all objects of that type are contained.
            FilteredIds = new HashSet<FreeformIdentity>(ids);

            // Set the parameters of the subgraph.
            // Note that we cannot use the children argument for non-dynamic parents.
            if (DynamicParent is null && children)
                throw new ArgumentException($"Cannot specify '{nameof(children)}' argument for non-dynamic parent.");
            Children = children;
            Computed = computed;

            // Compute the subgraph if requested.
            if (Computed) ComputeSubgraph();
        }
        /// <summary>
        /// Initializes an instance of the <see cref="FreeformSubgraph"/> object of a parent
        /// <see cref="FreeformGraph"/>.
        /// </summary>
        /// <remarks>
        /// Initializing the <see cref="FreeformSubgraph"/> with this constructor will contain all vertices and all
        /// edges.
        /// </remarks>
        /// <param name="parent">The parent graph.</param>
        /// <param name="children">
        /// Whether or not the children of the filtered identifiers should be contained instead of the vertex itself.
        /// </param>
        /// <param name="computed">
        /// Whether or not to compute the subgraph on initialization. This may improve performance but requires memory.
        /// </param>
        public FreeformSubgraph(FreeformGraph parent, bool children = false, bool computed = false)
            : this(parent, null, children: children, computed: computed) { }

        /// <summary>
        /// Computes the subgraph from the filtered identifiers.
        /// </summary>
        private void ComputeSubgraph()
        {
            if (DynamicParent is null)
            {
                // If there is parent graph is not dynamic, we simply perform a filter on the vertices and edges.
                ComputedVertices = new HashSet<FreeformVertex>
                (
                    Parent.Vertices.Where(vertex => (FilteredIds is null) || (FilteredIds.Contains(vertex.Identifier)))
                );
                ComputedEdges = new HashSet<FreeformEdge>
                (
                    Parent.Edges.Where(edge => (FilteredIds is null) || (FilteredIds.Contains(edge.Source.Identifier)))
                );
            }
            else
            {
                // If the parent graph is dynamic, we can use optimized calls to get the vertices and edges.
                if (FilteredIds is null)
                {
                    ComputedVertices = new HashSet<FreeformVertex>(DynamicParent.Vertices);
                    ComputedEdges = new HashSet<FreeformEdge>(DynamicParent.Edges);
                }
                else
                {
                    ComputedVertices = new HashSet<FreeformVertex>();
                    ComputedEdges = new HashSet<FreeformEdge>();
                    if (Children)
                    {
                        foreach (FreeformIdentity id in FilteredIds)
                        {
                            (IEnumerable<FreeformVertex> vertices, IEnumerable<FreeformEdge> edges) = DynamicParent.GetChildVerticesWithEdges(id);
                            foreach (FreeformVertex vertex in vertices)
                                ComputedVertices.Add(vertex);
                            foreach (FreeformEdge edge in edges)
                                ComputedEdges.Add(edge);
                        }
                    }
                    else
                    {
                        foreach (FreeformIdentity id in FilteredIds)
                        {
                            (FreeformVertex vertex, IEnumerable<FreeformEdge> edges) = DynamicParent.GetVertexWithEdges(id);
                            ComputedVertices.Add(vertex);
                            foreach (FreeformEdge edge in edges)
                                ComputedEdges.Add(edge);
                        }
                    }
                }
            }
        }

        #region FreeformGraph
        /// <inheritdoc />
        public override bool IsDirected => Parent.IsDirected;
        /// <inheritdoc />
        public override bool AllowParallelEdges => Parent.AllowParallelEdges;

        /// <inheritdoc />
        public override bool IsVerticesEmpty => Vertices.Any();
        /// <inheritdoc />
        public override bool IsEdgesEmpty => Edges.Any();

        /// <inheritdoc />
        public override int VertexCount => Vertices.Count();
        /// <inheritdoc />
        public override int EdgeCount => Edges.Count();

        /// <inheritdoc />
        public override IEnumerable<FreeformVertex> Vertices
        {
            get
            {
                // If we precomputed the vertices, simply return those.
                if (Computed)
                {
                    foreach (FreeformVertex vertex in ComputedVertices)
                        yield return vertex;
                    yield break;
                }

                // We can do an optimization by searching vertices instead of traversal for dynamic graphs.
                if (!(DynamicParent is null) && !(FilteredIds is null))
                {
                    if (Children)
                    {
                        foreach (FreeformIdentity id in FilteredIds)
                            foreach (FreeformVertex vertex in DynamicParent.GetChildVertices(id))
                                yield return vertex;
                    }
                    else
                    {
                        foreach (FreeformIdentity id in FilteredIds)
                            yield return DynamicParent.GetVertex(id);
                    }
                }
                else
                {
                    foreach (FreeformVertex vertex in Parent.Vertices)
                        if (FilteredIds.Contains(vertex.Identifier))
                            yield return vertex;
                }
            }
        }
        /// <inheritdoc />
        public override IEnumerable<FreeformEdge> Edges
        {
            get
            {
                // If we precomputed the edges, simply return those.
                if (Computed)
                {
                    foreach (FreeformEdge edge in ComputedEdges)
                        yield return edge;
                    yield break;
                }

                // We can do an optimization by searching edges instead of traversal for dynamic graphs.
                if (!(DynamicParent is null) && !(FilteredIds is null))
                {
                    // We get all edges connected to contained vertices.
                    if (Children)
                    {
                        foreach (FreeformIdentity id in FilteredIds)
                            foreach (FreeformEdge edge in DynamicParent.GetChildEdges(id))
                                yield return edge;
                    }
                    else
                    {
                        foreach (FreeformIdentity id in FilteredIds)
                            foreach (FreeformEdge edge in DynamicParent.GetEdges(id))
                                yield return edge;
                    }
                }
                else
                {
                    foreach (FreeformEdge edge in Parent.Edges)
                        if (FilteredIds.Contains(edge.Source.Identifier))
                            yield return edge;
                }
            }
        }

        /// <inheritdoc />
        public override bool ContainsVertex(FreeformVertex vertex)
        {
            // If we precomputed the vertices, simply yield those results.
            if (Computed)
                return ComputedVertices.Contains(vertex);

            // If we are filtering children instead of parents, we need to use special logic.
            if (Children)
            {
                foreach (FreeformIdentity id in FilteredIds)
                    if (DynamicParent.GetChildVertices(id).Any(childVertex => childVertex == vertex))
                        return true;
                return false;
            }

            // Apply the filter and then get the parent graph's containment.
            if ((FilteredIds is null) || FilteredIds.Contains(vertex.Identifier))
                return Parent.ContainsVertex(vertex);
            return false;
        }
        /// <inheritdoc />
        public override bool ContainsEdge(FreeformEdge edge)
        {
            // If we precomputed the edges, simply yield those results.
            if (Computed)
                return ComputedEdges.Any
                (
                    computedEdge =>
                        computedEdge.Source == edge.Source &&
                        computedEdge.Target == edge.Target
                );

            // If we are filtering children instead of parents, we need to use special logic.
            if (Children)
            {
                foreach (FreeformIdentity id in FilteredIds)
                    if (DynamicParent.GetChildEdges(id).Any(
                        childEdge =>
                            childEdge.Source == edge.Source &&
                            childEdge.Target == edge.Target
                    )) return true;
                return false;
            }

            // Apply the filter and then get the parent graph's containment.
            if ((FilteredIds is null) || FilteredIds.Contains(edge.Source.Identifier))
                return Parent.ContainsEdge(edge);
            return false;
        }
        #endregion FreeformGraph
    }
}