namespace CartaCore.Data.Synthetic
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
        /// Whether the nodes in the graph should be labeled.
        /// </summary>
        public bool Labeled { get; set; } = true;

        /// <summary>
        /// The minimum number (inclusive) of vertices a generated graph can have. 
        /// </summary>
        /// <value>The mininum number of vertices.</value>
        public int MinVertices { get; set; } = 1;
        /// <summary>
        /// The maximum number (inclusive) of vertices a generated graph can have.
        /// </summary>
        /// <value>The maximum number of vertices.</value>
        public int MaxVertices { get; set; } = 10;

        /// <summary>
        /// The minimum number (inclusive) of edges a generated graph can have. 
        /// </summary>
        /// <value>The mininum number of edges.</value>
        public int MinEdges { get; set; } = 0;
        /// <summary>
        /// The maximum number (inclusive) of edges a generated graph can have.
        /// </summary>
        /// <value>The maximum number of vertices.</value>
        public int MaxEdges { get; set; } = 50;
    }
}