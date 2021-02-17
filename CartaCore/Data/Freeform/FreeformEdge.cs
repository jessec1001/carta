using System;
using System.Diagnostics.CodeAnalysis;

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
        /// <param name="source">The source vertex.</param>
        /// <param name="target">The target vertex.</param>
        /// <param name="edgeId">The edge identifier.</param>
        public FreeformEdge(FreeformVertex source, FreeformVertex target, FreeformIdentity edgeId)
            : base(edgeId)
        {
            Source = source;
            Target = target;
        }
        /// <summary>
        /// Initializes an instance of the <see cref="FreeformEdge"/> class with the specified endpoints and index.
        /// </summary>
        /// <param name="source">The source vertex.</param>
        /// <param name="target">The target vertex.</param>
        /// <param name="edgeIndex">The edge index of the source vertex.</param>
        public FreeformEdge(FreeformVertex source, FreeformVertex target, int edgeIndex)
            : this(
                source, target,
                FreeformIdentity.Create(new FreeformVertexEdgeId(source.Identifier, edgeIndex))
            )
        { }
        /// <summary>
        /// Initializes an instance of the <see cref="FreeformEdge"/> class with the specified endpoints and identifier.
        /// </summary>
        /// <param name="sourceId">The source vertex identifier.</param>
        /// <param name="targetId">The target vertex identifier.</param>
        /// <param name="edgeId">The edge identifier.</param>
        public FreeformEdge(FreeformIdentity sourceId, FreeformIdentity targetId, FreeformIdentity edgeId)
            : this(
                new FreeformVertex(sourceId),
                new FreeformVertex(targetId),
                edgeId
            )
        { }
        /// <summary>
        /// Initializes an instance of the <see cref="FreeformEdge"/> class with the specified endpoints and index.
        /// </summary>
        /// <param name="sourceId">The source vertex identifier.</param>
        /// <param name="targetId">The target vertex identifier.</param>
        /// <param name="edgeIndex">The edge index of the source vertex.</param>
        public FreeformEdge(FreeformIdentity sourceId, FreeformIdentity targetId, int edgeIndex)
            : this(
                sourceId, targetId,
                FreeformIdentity.Create(new FreeformVertexEdgeId(sourceId, edgeIndex))
            )
        { }
    }

    /// <summary>
    /// Represents an identifier that uniquely represents an edge relative to its source vertex.
    /// </summary>
    public class FreeformVertexEdgeId : IEquatable<FreeformVertexEdgeId>, IComparable<FreeformVertexEdgeId>
    {
        /// <summary>
        /// Gets or sets the identifier of the source vertex.
        /// </summary>
        /// <value>The source vertex identifier.</value>
        public FreeformIdentity VertexIdentifier { get; protected set; }
        /// <summary>
        /// Gets or sets the index of the edge relative to its source index.
        /// </summary>
        /// <value>The edge index.</value>
        public int EdgeIndex { get; protected set; }

        /// <summary>
        /// Initializes an instance of the <see cref="FreeformVertexEdgeId"/> class with the specified vertex and edge
        /// identifier and index.
        /// </summary>
        /// <param name="vertexId">The identifier of the source vertex.</param>
        /// <param name="edgeIndex">The index of the edge relative to its source index.</param>
        public FreeformVertexEdgeId(FreeformIdentity vertexId, int edgeIndex)
        {
            VertexIdentifier = vertexId;
            EdgeIndex = edgeIndex;
        }

        /// <inheritdoc />
        public bool Equals([AllowNull] FreeformVertexEdgeId other)
        {
            return
                VertexIdentifier.Equals(other.VertexIdentifier) &&
                EdgeIndex.Equals(other.EdgeIndex);
        }
        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is FreeformVertexEdgeId other) return Equals(other);
            return false;
        }

        /// <inheritdoc />
        public int CompareTo([AllowNull] FreeformVertexEdgeId other)
        {
            // We compare vertex identifier first and edge index second.
            int compareVertex = VertexIdentifier.CompareTo(other.VertexIdentifier);
            if (compareVertex == 0)
            {
                int compareEdge = EdgeIndex.CompareTo(other.EdgeIndex);
                return compareEdge;
            }
            return compareVertex;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            // Offset the vertex identifier hash with a random multiple of the edge index.
            int hash = VertexIdentifier.GetHashCode();
            hash += EdgeIndex * 0x04df3799;
            return hash;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{VertexIdentifier}.{EdgeIndex}";
        }

        /// <summary>
        /// Converts the string representation of an edge identity to its <see cref="FreeformVertexEdgeId" /> object
        /// equivalent. A return value indicates whether the operation succeeded.
        /// </summary>
        /// <param name="source">A string containing the edge identity to convert.</param>
        /// <param name="id">
        /// Contains either the parsed edge identity if the conversion succeeded or <c>null</c> if the conversion
        /// failed.
        /// </param>
        /// <returns><c>true</c> if the conversion was successful; otherwise <c>false</c>.</returns>
        public static bool TryParse(string source, out FreeformVertexEdgeId id)
        {
            // Set the edge identity to null in case we cannot parse the string representation.
            id = null;

            // The string is of the form "vertex.#edge".
            // We try to parse each part separately.
            int dividerIndex = source.LastIndexOf('.');
            if (dividerIndex >= 0)
            {
                FreeformIdentity vertexId = FreeformIdentity.Create(source.Substring(0, dividerIndex));
                bool success = int.TryParse(source.Substring(dividerIndex + 1), out int edgeIndex);

                if (success)
                {
                    id = new FreeformVertexEdgeId(vertexId, edgeIndex);
                    return true;
                }
            }

            return false;
        }
    }
}