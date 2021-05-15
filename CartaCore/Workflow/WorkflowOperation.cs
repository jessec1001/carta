using CartaCore.Workflow.Action;
using CartaCore.Workflow.Selection;

namespace CartaCore.Workflow
{
    /// <summary>
    /// Represents a single operation in the sequence of operations for a workflow. Consists of a selector followed by
    /// an action that allows targeted transformations of data.
    /// </summary>
    public class WorkflowOperation
    {
        /// <summary>
        /// Gets or sets the workflow operation name.
        /// </summary>
        /// <value>A human-readable name for the workflow operation.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the selector for the operation.
        /// </summary>
        /// <value>
        /// The selector that selects vertices, edges, and properties for which the operation is applicable. The
        /// selected elements are passed on to the action. If not specified, defaults to select all.
        /// </value>
        public Selector Selector { get; set; }
        /// <summary>
        /// Gets or sets the action for the operation.
        /// </summary>
        /// <value>
        /// The action that acts on vertices, edges, and properties specified by the selector. If not specified, there
        /// is no action applied.
        /// </value>
        public Actor Actor { get; set; }
    }
}