namespace CartaCore.Data
{
    /// <summary>
    /// Represents the base functionality and characteristics of a graph object.
    /// </summary>
    public interface IGraph : IProvider<IGraph>
    {
        /// <summary>
        /// Gets the structure properties of the graph.
        /// </summary>
        /// <returns>The graph properties.</returns>
        GraphProperties GetProperties();
    }
}