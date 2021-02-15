using System;

using QuikGraph;

namespace CartaCore.Data.Freeform
{
    /// <summary>
    /// Represents a graph edge that can be identified and connects precisely a pair of vertices together.
    /// </summary>
    public class FreeformEdge : FreeformObjectBase<FreeformEdge>, IEdge<FreeformVertex>
    {
        /// <inheritdoc />
        public FreeformVertex Source { get; protected set; }
        /// <inheritdoc />
        public FreeformVertex Target { get; protected set; }

        /// <summary>
        /// Initializes an instance of the <see cref="FreeformEdge"/> class with the specified endpoints and identifier.
        /// </summary>
        /// <param name="sourceId">The source vertex identifier.</param>
        /// <param name="targetId">The target vertex identifier.</param>
        /// <param name="edgeId">The edge identifier.</param>
        public FreeformEdge(FreeformIdentity sourceId, FreeformIdentity targetId, FreeformIdentity edgeId)
            : base(edgeId)
        {
            Source = new FreeformVertex(sourceId);
            Target = new FreeformVertex(targetId);
        }
    }
}