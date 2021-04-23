using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CartaCore.Data;
using CartaCore.Workflow.Action;
using CartaCore.Workflow.Selection;

namespace CartaCore.Workflow
{
    /// <summary>
    /// Represents a linear sequence of operations that can be performed on a graph object. These operations can be
    /// performed asynchronously.
    /// </summary>
    public class Workflow
    {
        /// <summary>
        /// Gets or sets the stored identifier for the workflow.
        /// </summary>
        /// <value>The workflow identifier.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the workflow name.
        /// </summary>
        /// <value>A human-readable name for the workflow.</value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the workflow operations.
        /// </summary>
        /// <returns></returns>
        public List<WorkflowOperation> Operations { get; set; } = null;

        /// <summary>
        /// Applies the workflow to a finite graph and returns another graph with the operations of the workflow
        /// applied.
        /// </summary>
        /// <param name="graph">The graph to apply the workflow to.</param>
        /// <returns>The graph after being acted on by the workflow operations.</returns>
        public IGraph Apply(IGraph graph)
        {
            if (Operations is null) return graph;

            foreach (WorkflowOperation operation in Operations)
            {
                Actor actor = operation.Actor;
                Selector selector = operation.Selector;

                // Wrap graph references.
                actor.Graph = graph;
                selector.Graph = graph;
                actor.Selector = selector;
                graph = actor;
            }

            return graph;
        }
    }
}