using CartaCore.Graphs.Components;

namespace CartaCore.Graphs
{
    /// <summary>
    /// Represents the base functionality and characteristics of a graph object.
    /// </summary>
    public interface IGraph
    {
        /// <summary>
        /// The structural attributes of the graph.
        /// </summary>
        GraphAttributes Attributes { get; }
        /// <summary>
        /// The components attached to the graph.
        /// </summary>
        ComponentStack Components { get; }
    }
}