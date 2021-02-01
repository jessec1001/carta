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
        /// Initializes a new instance of the <see cref="FreeformEdge"/> class.
        /// </summary>
        /// <param name="source">The source vertex.</param>
        /// <param name="target">The target vertex.</param>
        public FreeformEdge(FreeformVertex source, FreeformVertex target)
            : base(source, target) { }
    }
}