using CartaCore.Data;
using CartaCore.Serialization.Json;

namespace CartaCore.Workflow.Action
{
    /// <summary>
    /// Represents an action that can act on or transform vertices, edges, or a graph itself.
    /// </summary>
    [DiscriminantBase]
    public abstract class ActionBase
    {
        /// <summary>
        /// Applies the action to a specified vertex.
        /// </summary>
        /// <param name="vertex">The vertex to apply to action to.</param>
        /// <returns>The vertex after the action has been applied.</returns>
        public abstract IVertex ApplyToVertex(IVertex vertex);
    }
}