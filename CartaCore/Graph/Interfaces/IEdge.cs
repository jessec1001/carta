namespace CartaCore.Graph
{
    /// <summary>
    /// Represents the base structure of an edge in a graph.
    /// </summary>
    public interface IEdge : IIdentifiable
    {
        /// <summary>
        /// Whether or not this edge is directed. Defaults to true.
        /// </summary>
        bool Directed { get; }

        /// <summary>
        /// The unique identifier of the source vertex of the edge. 
        /// </summary>
        string Source { get; }
        /// <summary>
        /// The unique identifier of the target vertex of the edge.
        /// </summary>
        string Target { get; }
    }
}