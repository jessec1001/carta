namespace CartaCore.Data
{
    /// <summary>
    /// Represents the properties that a graph can take on that determine how the graph must be retrieved or processed.
    /// </summary>
    public struct GraphProperties
    {
        /// <summary>
        /// Gets whether the graph contains directed or undirected edges.
        /// </summary>
        /// <value>
        /// <c>true</c> if the edges are directed; <c>false</c> if the edges are undirected. If the edges are directed,
        /// the order of <see cref="Edge.Source"/> and <see cref="Edge.Target"/> matter and change the flow of the edge.
        /// </value>
        public bool Directed { get; init; }
        /// <summary>
        /// Gets whether the graph can be loaded dynamically.
        /// </summary>
        /// <value>
        /// <c>true</c> if the graph and its vertices can be loaded dynamically.
        /// </value>
        public bool Dynamic { get; init; }
        /// <summary>
        /// Gets whether the graph is finite or infinite.
        /// </summary>
        /// <value>
        /// <c>true</c> if the graph contains finitely many vertices and edges; <c>false</c> is the graph contains
        /// infinitely many vertices or edges. This should be used to determine whether an operation can be performed
        /// on the graph in a computationally finite amount of time.
        /// </value>
        public bool Finite { get; init; }
    }
}