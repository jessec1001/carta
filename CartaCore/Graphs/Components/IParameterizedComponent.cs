namespace CartaCore.Graphs.Components
{
    /// <summary>
    /// A graph component that is parameterized.
    /// </summary>
    /// <typeparam name="TParameters">The type of parameters provided.</typeparam>
    public interface IParameterizedComponent<TParameters> : IComponent where TParameters : new()
    {
        /// <summary>
        /// The parameters used to generate the graph.
        /// </summary>
        TParameters Parameters { get; }
    }
}