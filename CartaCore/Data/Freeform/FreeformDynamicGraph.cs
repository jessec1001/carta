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