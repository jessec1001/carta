using CartaCore.Statistics;

namespace CartaCore.Data.Synthetic
{
    /// <summary>
    /// Represents the generation options of the <see cref="RandomInfiniteDirectedGraph"/> object.
    /// </summary>
    public class RandomInfiniteDirectedGraphOptions
    {
        /// <summary>
        /// Gets or sets the seed for random generation of the graph.
        /// </summary>
        /// <value>The random seed.</value>
        public ulong Seed { get; set; } = 0;

        /// <summary>
        /// Gets or sets the distributioon of the number of properties generated for the entire graph.
        /// </summary>
        /// <value>The property count distribution.</value>
        public IIntegerDistribution PropertyCount { get; set; } = new PoissonDistribution(20);
        /// <summary>
        /// Gets or sets the probability that a property is included in a vertex.
        /// </summary>
        /// <value>The property inclusion probability.</value>
        public double PropertyInclusionProbability { get; set; } = 0.75;

        /// <summary>
        /// Gets or sets the distribution of the number of children each node should generate in the graph.
        /// </summary>
        /// <returns>The child count distribution.</returns>
        public IIntegerDistribution ChildCount { get; set; } = new PoissonDistribution(2);
    }
}