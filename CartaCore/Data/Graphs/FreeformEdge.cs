using QuikGraph;

namespace CartaCore.Data.Graphs
{
    /// <summary>
    /// Represents an edge between freeform vertices.
    /// </summary>
    /// <seealso cref="FreeformId" />
    /// <seealso cref="FreeformVertex" />
    public class FreeformEdge : EquatableEdge<FreeformId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FreeformEdge"/> class.
        /// </summary>
        /// <param name="source">The source vertex.</param>
        /// <param name="target">The target vertex.</param>
        public FreeformEdge(FreeformId source, FreeformId target)
            : base(source, target) { }
    }
}