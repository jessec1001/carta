using System.Collections.Generic;
using System.Threading.Tasks;

using CartaCore.Data;
using CartaCore.Serialization;

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
        public async override Task<IVertex> ApplyToVertex(IGraph graph, IVertex vertex)
        {
            foreach (ActionBase action in Actions)
                vertex = await action.ApplyToVertex(graph, vertex);
            return vertex;
        }
    }
}