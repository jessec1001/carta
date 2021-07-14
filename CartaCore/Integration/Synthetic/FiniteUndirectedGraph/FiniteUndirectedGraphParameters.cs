using CartaCore.Statistics;

namespace CartaCore.Integration.Synthetic
{
    /// <summary>
    /// Represents the generation parameters of the <see cref="FiniteUndirectedGraph"/> object.
    /// </summary>
    public class FiniteUndirectedGraphParameters
    {
        /// <summary>
        /// The seed for random generation of the graph.
        /// </summary>
        /// <value>The random seed.</value>
        public ulong Seed { get; set; } = 0;

        /// <summary>
        /// Whether the vertices in the graph should be labeled.
        /// </summary>
        public bool Labeled { get; set; } = true;

        /// <summary>
        /// The minimum number (inclusive) of edges a generated graph can have. 
        /// </summary>
        /// <returns>The vertex count distribution.</returns>
        public IIntegerDistribution VertexCount { get; set; } = new PoissonDistribution(10);
        /// <summary>
        /// Gets or sets the distribution of the number of edges a generated graph will have.
        /// </summary>
        /// <returns>The edge count distribution.</returns>
        public IIntegerDistribution EdgeCount { get; set; } = new PoissonDistribution(30);
    }
}