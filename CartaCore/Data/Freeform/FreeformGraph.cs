using System.Collections.Generic;

using QuikGraph;

namespace CartaCore.Data.Freeform
{
    /// <summary>
    /// Represents a graph structure that stores flexible vertex and edge values with numerous properties and
    /// observations.
    /// </summary>
    public abstract class FreeformGraph : IEdgeListGraph<FreeformVertex, FreeformEdge>
    {
        #region IEdgeListGraph<FreeformVertex, FreefromEdge>
        /// <inheritdoc />
        public abstract bool IsDirected { get; }
        /// <inheritdoc />
        public abstract bool AllowParallelEdges { get; }

        /// <inheritdoc />
        public abstract bool IsVerticesEmpty { get; }
        /// <inheritdoc />
        public abstract bool IsEdgesEmpty { get; }

        /// <inheritdoc />
        public abstract int VertexCount { get; }
        /// <inheritdoc />
        public abstract int EdgeCount { get; }

        /// <inheritdoc />
        public abstract IEnumerable<FreeformVertex> Vertices { get; }
        /// <inheritdoc />
        public abstract IEnumerable<FreeformEdge> Edges { get; }

        /// <inheritdoc />
        public abstract bool ContainsVertex(FreeformVertex vertex);
        /// <inheritdoc />
        public abstract bool ContainsEdge(FreeformEdge edge);
        #endregion
    }
}