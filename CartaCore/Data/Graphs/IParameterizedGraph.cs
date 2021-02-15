namespace CartaCore.Data
{
    /// <summary>
    /// Represents graph data that is parameterized.
    /// </summary>
    /// <typeparam name="TParameters">The type of parameters provided to the data.</typeparam>
    public interface IParameterizedGraph<TParameters> where TParameters : new()
    {
        /// <summary>
        /// The parameters used to generate the graph.
        /// </summary>
        TParameters Parameters { get; set; }
    }
}