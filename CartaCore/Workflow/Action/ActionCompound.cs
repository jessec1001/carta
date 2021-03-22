using System.Collections.Generic;

using CartaCore.Data;
using CartaCore.Serialization.Json;

namespace CartaCore.Workflow.Action
{
    /// <summary>
    /// Represents an action that is a sequential combination of multiple actions to apply to a vertex.
    /// </summary>
    [DiscriminantDerived("compound")]
    public class ActionCompound : ActionBase
    {
        /// <summary>
        /// Gets or sets the list of actions to perform on a vertex.
        /// </summary>
        /// <returns>The list of actions that are performed sequentially on the vertex.</returns>
        public List<ActionBase> Actions { get; set; } = new List<ActionBase>();

        /// <inheritdoc />
        public override IVertex ApplyToVertex(IVertex vertex)
        {
            foreach (ActionBase action in Actions)
                vertex = action.ApplyToVertex(vertex);
            return vertex;
        }
    }
}