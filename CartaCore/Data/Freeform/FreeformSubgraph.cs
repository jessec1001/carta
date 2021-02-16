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
        public ISet<FreeformIdentity> ContainedVertices { get; protected set; }
        /// <summary>
        /// Gets or sets the edge identifiers contained in this subgraph.
        /// </summary>
        /// <value>The contained edge identifiers.</value>
        public ISet<FreeformIdentity> ContainedEdges { get; protected set; }

        /// <summary>
        /// Initializes an instance of the <see cref="FreeformSubgraph"/> object of a parent <see cref="FreeformGraph"/>
        /// with the specified contained vertices and edges.
        /// </summary>
        /// <remarks>
        /// If either the contained vertices or contained edges are set to <c>null</c>, then all of the respective
        /// object are contained.
        /// </remarks>
        /// <param name="parent">The parent graph.</param>
        /// <param name="vertices">The contained vertices. May be <c>null</c>.</param>
        /// <param name="edges">The contained edges. May be <c>null</c>.</param>
        public FreeformSubgraph(FreeformGraph parent, IEnumerable<FreeformIdentity> vertices, IEnumerable<FreeformIdentity> edges)
        {
            Parent = parent;
            DynamicParent = parent as FreeformDynamicGraph;

            // Set the contained vertices and edges.
            // A value of null for either means all objects of that type are contained.
            ContainedVertices = new HashSet<FreeformIdentity>(vertices);
            ContainedEdges = new HashSet<FreeformIdentity>(edges);
        }
        /// <summary>
        /// Initializes an instance of the <see cref="FreeformSubgraph"/> object of a parent <see cref="FreeformGraph"/>
        /// with the specified contained vertices.
        /// </summary>
        /// <remarks>
        /// Initializing the <see cref="FreeformSubgraph"/> with this constructor will contain all edges. If the
        /// contained vertices are set to <c>null</c>, then all of the vertices are contained.
        /// </remarks>
        /// <param name="parent">The parent graph.</param>
        /// <param name="vertices">The contained vertices. May be <c>null</c>.</param>
        public FreeformSubgraph(FreeformGraph parent, IEnumerable<FreeformIdentity> vertices)
            : this(parent, vertices, null) { }
        /// <summary>
        /// Initializes an instance of the <see cref="FreeformSubgraph"/> object of a parent
        /// <see cref="FreeformGraph"/>.
        /// </summary>
        /// <remarks>
        /// Initializing the <see cref="FreeformSubgraph"/> with this constructor will contain all vertices and all
        /// edges.
        /// </remarks>
        /// <param name="parent">The parent graph.</param>
        public FreeformSubgraph(FreeformGraph parent)
            : this(parent, null, null) { }

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
                // We can do an optimization by searching vertices instead of traversal for dynamic graphs.
                if (!(DynamicParent is null) && !(ContainedVertices is null))
                {
                    foreach (FreeformIdentity id in ContainedVertices)
                        yield return DynamicParent.GetVertex(id);
                }
                else
                {
                    foreach (FreeformVertex vertex in Parent.Vertices)
                        yield return vertex;
                }
            }
        }
        /// <inheritdoc />
        public override IEnumerable<FreeformEdge> Edges
        {
            get
            {
                // We can do an optimization by searching edges instead of traversal for dynamic graphs.
                if (!(DynamicParent is null) && !(ContainedVertices is null))
                {
                    // If we don't specify the contained edges, we get all connected to contained vertices.
                    foreach (FreeformIdentity id in ContainedVertices)
                        foreach (FreeformEdge edge in DynamicParent.GetEdges(id))
                            if (ContainedEdges is null || ContainedEdges.Contains(edge.Identifier))
                                yield return edge;
                }
                else
                {
                    foreach (FreeformEdge edge in Parent.Edges)
                        yield return edge;
                }
            }
        }

        /// <inheritdoc />
        public override bool ContainsVertex(FreeformVertex vertex)
        {
            if ((ContainedVertices is null) || ContainedVertices.Contains(vertex.Identifier))
                return Parent.ContainsVertex(vertex);
            return false;
        }
        /// <inheritdoc />
        public override bool ContainsEdge(FreeformEdge edge)
        {
            if ((ContainedEdges is null) || ContainedEdges.Contains(edge.Identifier))
                return Parent.ContainsEdge(edge);
            return false;
        }
        #endregion FreeformGraph
    }
}