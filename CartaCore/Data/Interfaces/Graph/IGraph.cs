namespace CartaCore.Data
{
    /// <summary>
    /// Represents the base functionality and characteristics of a graph object.
    /// </summary>
    public interface IGraph : IProvider<IGraph>
    {
        /// <summary>
        /// The structural attributes of the graph.
        /// </summary>
        GraphAttributes Attributes { get; }
    }
}