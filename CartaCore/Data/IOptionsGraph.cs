namespace CartaCore.Data
{
    /// <summary>
    /// Represents graph data that is parameterized using some options.
    /// </summary>
    /// <typeparam name="TOptions">The type of options provided to the data.</typeparam>
    public interface IOptionsGraph<TOptions> where TOptions : new()
    {
        /// <summary>
        /// The options used to generate the graph.
        /// </summary>
        TOptions Options { get; set; }
    }
}