using QuikGraph;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents an edge between freeform vertices.
    /// </summary>
    /// <seealso cref="FreeformVertex" />
    public class FreeformEdge : EquatableEdge<FreeformVertex>
    {
        /// <summary>
        /// Gets or sets the edge identifier.
        /// </summary>
        /// <value>
        /// The edge identifier. The identifier should be globally unique within a graph or unique as an out-edge of the
        /// source vertex.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FreeformEdge"/> class.
        /// </summary>
        /// <param name="source">The source vertex.</param>
        /// <param name="target">The target vertex.</param>
        /// <param name="id">The edge identifier. Should be unique within a scope.</param>
        public FreeformEdge(FreeformVertex source, FreeformVertex target, int id)
            : base(source, target)
        {
            Id = id;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="FreeformEdge"/> class.
        /// </summary>
        /// <param name="source">The source vertex.</param>
        /// <param name="target">The target vertex.</param>
        public FreeformEdge(FreeformVertex source, FreeformVertex target)
            : base(source, target) { }
    }
}