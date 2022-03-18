using CartaCore.Graphs.Components;

namespace CartaCore.Graphs
{
    /// <summary>
    /// Represents the base functionality and characteristics of a graph object.
    /// </summary>
    public interface IGraph
    {
        /// <summary>
        /// The components attached to the graph.
        /// </summary>
        ComponentStack Components { get; }
    }
}