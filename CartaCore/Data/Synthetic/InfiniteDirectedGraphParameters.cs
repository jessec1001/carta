namespace CartaCore.Data.Synthetic
{
    /// <summary>
    /// Represents the generation parameters of the <see cref="InfiniteDirectedGraph"/> object.
    /// </summary>
    public class InfiniteDirectedGraphParameters
    {
        /// <summary>
        /// The seed for random generation of the graph.
        /// </summary>
        public ulong Seed { get; set; } = 0;

        /// <summary>
        /// The number of properties generated for the entire graph.
        /// </summary>
        public int PropertyCount { get; set; } = 20;
        /// <summary>
        /// The average percentage of the total properties that any particular vertex obtains.
        /// </summary>
        public double PropertyDensity { get; set; } = 0.70;

        /// <summary>
        /// The initial probability of a vertex constructing an edge to a child.
        /// </summary>
        public double ChildProbability { get; set; } = 1.00;
        /// <summary>
        /// The amount that the child probability gets scaled by each time a child is generated.
        /// </summary>
        public double ChildDampener { get; set; } = 0.50;
    }
}