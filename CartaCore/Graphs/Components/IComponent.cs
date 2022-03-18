namespace CartaCore.Graphs.Components
{
    /// <summary>
    /// Represents a component that can be added to a graph.
    /// </summary>
    public interface IComponent
    {
        /// <summary>
        /// The component stack available to the component.
        /// This will be set automatically when the component is added to the graph.
        /// </summary>
        public ComponentStack Components { get; set; }
    }
}