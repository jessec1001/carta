using CartaCore.Statistics;

namespace CartaCore.Integration.Synthetic
{
    /// <summary>
    /// Represents the generation parameters of the <see cref="InfiniteDirectedGraph"/> object.
    /// </summary>
    public class InfiniteDirectedGraphParameters
    {
        /// <summary>
        /// Gets or sets the seed for random generation of the graph.
        /// </summary>
        /// <value>The random seed.</value>
        public ulong Seed { get; set; } = 0;

        /// <summary>
        /// Whether the vertices in the graph should be labeled.
        /// </summary>
        public bool Labeled { get; set; } = true;

        /// <summary>
        /// The number of properties generated for the entire graph.
        /// </summary>
        /// <value>The property count distribution.</value>
        public int PropertyCount { get; set; } = 20;
        /// <summary>
        /// Gets or sets the probability that a property is included in a vertex.
        /// </summary>
        /// <value>The property inclusion probability.</value>
        public double PropertyInclusionProbability { get; set; } = 0.75;

        /// <summary>
        /// Gets or sets the distribution of the number of children each vertex should generate in the graph.
        /// </summary>
        /// <returns>The child count distribution.</returns>
        public int ChildCount { get; set; } = 2;
    }
}