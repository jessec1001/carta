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