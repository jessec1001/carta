using CartaCore.Statistics;

namespace CartaCore.Data.Synthetic
{
    /// <summary>
    /// Represents the generation options of the <see cref="RandomFiniteUndirectedGraph"/> object.
    /// </summary>
    public class RandomFiniteUndirectedGraphOptions
    {
        /// <summary>
        /// The seed for random generation of the graph.
        /// </summary>
        /// <value>The random seed.</value>
        public ulong Seed { get; set; } = 0;

        /// <summary>
        /// Gets or sets the distribution of the number of vertices a generated graph will have.
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