using System;
using System.Collections.Generic;

namespace CartaCore.Data.Freeform
{
    /// <summary>
    /// Represents a <see cref="FreeformGraph"/> that should be read vertex-by-vertex rather than all at once.
    /// </summary>
    public abstract class FreeformDynamicGraph : FreeformGraph
    {
        /// <summary>
        /// Gets whether the graph has a finite or an infinite number of vertices and/or edges.
        /// </summary>
        /// <value><c>true</c> if the graph is finite; <c>false</c> if the graph is infinite.</value>
        public abstract bool IsFinite { get; }
        /// <summary>
        /// Gets the identifier for the base vertex of the graph.
        /// </summary>
        /// <value>The base or common ancestor node of the graph.</value>
        public abstract FreeformIdentity BaseId { get; }

        /// <inheritdoc />
        public override IEnumerable<FreeformVertex> Vertices => TraversePreorderVertices(BaseId);
        /// <inheritdoc />
        public override IEnumerable<FreeformEdge> Edges => TraversePreorderEdges(BaseId);

        /// <summary>
        /// Gets a vertex specified by an identifier.
        /// </summary>
        /// <param name="id">The vertex identifier.</param>
        /// <returns>
        /// If the vertex by the corresponding identifier exists, it is returned. Otherwise, returns <c>null</c>.
        /// </returns>
        public abstract FreeformVertex GetVertex(FreeformIdentity id);
        /// <summary>
        /// Gets the out-edges of a vertex specified by an identifier.
        /// </summary>
        /// <param name="id">The vertex identifier.</param>
        /// <returns>
        /// If the vertex by the corresponding identifier exists, the enumerable of the out-edges of the vertices.
        /// Otherwise, returns <c>null</c>.
        /// </returns>
        public abstract IEnumerable<FreeformEdge> GetEdges(FreeformIdentity id);

        /// <summary>
        /// Gets the child vertices of a vertex specified by an identifier.
        /// </summary>
        /// <param name="id">The vertex identifier.</param>
        /// <returns>
        /// If the vertex by the corresponding identifier exists, the child vertices are returned. Otherwise, returns
        /// <c>null</c>.
        /// </returns>
        public virtual IEnumerable<FreeformVertex> GetChildVertices(FreeformIdentity id)
        {
            foreach (FreeformEdge edge in GetEdges(id))
                yield return GetVertex(edge.Target.Identifier);
        }
        /// <summary>
        /// Gets the out-edges of child vertices of a vertex specified by an identifier.
        /// </summary>
        /// <param name="id">The vertex identifier.</param>
        /// <returns>
        /// If the vertex by the corresponding identifier exists, the enumerable of the out-edges of the child vertices.
        /// Otherwise, returns <c>null</c>.
        /// </returns>
        public virtual IEnumerable<FreeformEdge> GetChildEdges(FreeformIdentity id)
        {
            foreach (FreeformEdge edge in GetEdges(id))
            {
                foreach (FreeformEdge childEdge in GetEdges(edge.Target.Identifier))
                {
                    yield return childEdge;
                }
            }
        }

        /// <summary>
        /// Gets the preorder traversal of the graph vertices.
        /// </summary>
        /// <param name="id">The ancestor vertex identifier.</param>
        /// <returns>The preorder enumerable of vertices.</returns>
        public virtual IEnumerable<FreeformVertex> TraversePreorderVertices(FreeformIdentity id)
        {
            // Return the preorder traversal of the hierarchy.
            yield return GetVertex(id);
            foreach (FreeformEdge edge in GetEdges(id))
                foreach (FreeformVertex vertex in TraversePostorderVertices(edge.Target.Identifier))
                    yield return vertex;
        }
        /// <summary>
        /// Gets the preorder traversal of the graph edges.
        /// </summary>
        /// <param name="id">The ancestor vertex identifier.</param>
        /// <returns>The preorder enumerable of edges.</returns>
        public virtual IEnumerable<FreeformEdge> TraversePreorderEdges(FreeformIdentity id)
        {
            // Return the preorder traversal of the hierarchy.
            foreach (FreeformEdge edge in GetEdges(id))
            {
                yield return edge;
                foreach (FreeformEdge childEdge in TraversePreorderEdges(edge.Target.Identifier))
                    yield return childEdge;
            }
        }
        /// <summary>
        /// Gets the postorder traversal of the graph vertices.
        /// </summary>
        /// <param name="id">The ancestor vertex identifier.</param>
        /// <returns>The postorder enumerable of vertices.</returns>
        public virtual IEnumerable<FreeformVertex> TraversePostorderVertices(FreeformIdentity id)
        {
            // Return the postorder traversal of the hierarchy.
            foreach (FreeformEdge edge in GetEdges(id))
                foreach (FreeformVertex vertex in TraversePostorderVertices(edge.Target.Identifier))
                    yield return vertex;
            yield return GetVertex(id);
        }
        /// <summary>
        /// Gets the postorder traversal of the graph edges.
        /// </summary>
        /// <param name="id">The ancestor vertex identifier.</param>
        /// <returns>The postorder enumerable of edges.</returns>
        public virtual IEnumerable<FreeformEdge> TraversePostorderEdges(FreeformIdentity id)
        {
            // Return the postorder traversal of the hierarchy.
            foreach (FreeformEdge edge in GetEdges(id))
            {
                foreach (FreeformEdge childEdge in TraversePostorderEdges(edge.Target.Identifier))
                    yield return childEdge;
                yield return edge;
            }
        }
    }

    /// <summary>
    /// Represents a <see cref="FreeformGraph"/> that should be read vertex-by-vertex rather than all at once.
    /// </summary>
    /// <typeparam name="T">The type of identifier for vertices.</typeparam>
    public abstract class FreeformDynamicGraph<T> : FreeformDynamicGraph
        where T : IEquatable<T>, IComparable<T>
    {
        /// <summary>
        /// Gets a vertex specified by an identifier.
        /// </summary>
        /// <param name="id">The vertex identifier.</param>
        /// <returns>
        /// If it exists, the vertex with the corresponding identifier. Otherwise, returns <c>null</c>.
        /// </returns>
        public abstract FreeformVertex GetVertex(T id);
        /// <inheritdoc />
        public override FreeformVertex GetVertex(FreeformIdentity id)
        {
            if (FreeformIdentity.IsType(id, out T typedId)) return GetVertex(typedId);
            return null;
        }

        /// <summary>
        /// Gets the out-edges of a vertex specified by an identifier.
        /// </summary>
        /// <param name="id">The vertex identifier.</param>
        /// <returns>
        /// If the vertex by the corresponding identifier exists, the enumerable of the out-edges of the vertices.
        /// Otherwise, returns <c>null</c>.
        /// </returns>
        public abstract IEnumerable<FreeformEdge> GetEdges(T id);
        /// <inheritdoc />
        public override IEnumerable<FreeformEdge> GetEdges(FreeformIdentity id)
        {
            if (FreeformIdentity.IsType(id, out T typedId)) return GetEdges(typedId);
            return null;
        }

        /// <summary>
        /// Gets the child vertices of a vertex specified by an identifier.
        /// </summary>
        /// <param name="id">The vertex identifier.</param>
        /// <returns>
        /// If the vertex by the corresponding identifier exists, the child vertices are returned. Otherwise, returns
        /// <c>null</c>.
        /// </returns>
        public virtual IEnumerable<FreeformVertex> GetChildVertices(T id)
        {
            foreach (FreeformEdge edge in GetEdges(id))
                yield return GetVertex(edge.Target.Identifier);
        }
        /// <inheritdoc />
        public override IEnumerable<FreeformVertex> GetChildVertices(FreeformIdentity id)
        {
            if (FreeformIdentity.IsType(id, out T typedId)) return GetChildVertices(typedId);
            return null;
        }
        /// <summary>
        /// Gets the out-edges of child vertices of a vertex specified by an identifier.
        /// </summary>
        /// <param name="id">The vertex identifier.</param>
        /// <returns>
        /// If the vertex by the corresponding identifier exists, the enumerable of the out-edges of the child vertices.
        /// Otherwise, returns <c>null</c>.
        /// </returns>
        public virtual IEnumerable<FreeformEdge> GetChildEdges(T id)
        {
            foreach (FreeformEdge edge in GetEdges(id))
            {
                foreach (FreeformEdge childEdge in GetEdges(edge.Target.Identifier))
                {
                    yield return childEdge;
                }
            }
        }
        /// <inheritdoc />
        public override IEnumerable<FreeformEdge> GetChildEdges(FreeformIdentity id)
        {
            if (FreeformIdentity.IsType(id, out T typedId)) return GetChildEdges(typedId);
            return null;
        }
    }
}