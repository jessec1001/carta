using System;

namespace CartaWeb.Models.DocumentItem
{
    /// <summary>
    /// A point at which a workflow connection can attach to. Refers of a specific field on an operation.
    /// </summary>
    public class WorkflowConnectionPoint
    {
        /// <summary>
        /// The unique identifier of the operation.
        /// </summary>
        public string Operation { get; set; }
        /// <summary>
        /// The field on the operation to attach to.
        /// </summary>
        public string Field { get; set; }
    }
    /// <summary>
    /// A connection between two fields on distinct workflow operations indicating a passage of data.
    /// </summary>
    public class WorkflowConnection
    {
        /// <summary>
        /// The unique identifier of the connection.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The source point of the connection.
        /// </summary>
        public WorkflowConnectionPoint Source { get; set; }
        /// <summary>
        /// The target point of the connection.
        /// </summary>
        public WorkflowConnectionPoint Target { get; set; }
        
        /// <summary>
        /// Whether this connection should multiplex from source to target.
        /// </summary>
        public bool Multiplex { get; set; } = false;
    }

    /// <summary>
    /// An item that represents a workflow along with its suboperations and connections. This workflow item is then used
    /// to construct a workflow operation from its template.
    /// </summary>
    public class WorkflowItem : Item
    {
        /// <summary>
        /// The name of the workflow template.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The description of the workflow template.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The suboperations contained in the workflow.
        /// </summary>
        public string[] Operations { get; set; }
        /// <summary>
        /// The connections between the suboperations contained in the workflow.
        /// </summary>
        public WorkflowConnection[] Connections { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowItem"/> class.
        /// </summary>
        public WorkflowItem() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowItem"/> class with specified suboperations and
        /// connections.
        /// </summary>
        /// <param name="workflowId">The unique identifier of the workflow.</param>
        /// <param name="operations">The suboperations of the workflow.</param>
        /// <param name="connections">The connections of the workflow.</param>
        public WorkflowItem(string workflowId, string[] operations, WorkflowConnection[] connections)
            : base(workflowId)
        {
            Operations = operations;
            Connections = connections;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowItem"/> class with no suboperations or connections.
        /// </summary>
        /// <param name="workflowId">The unique identifier of the workflow.</param>
        public WorkflowItem(string workflowId)
            : base(workflowId)
        {
            Operations = Array.Empty<string>();
            Connections = Array.Empty<WorkflowConnection>();
        }

        /// <inheritdoc />
        public override string PartitionKeyPrefix => "WORKFLOW#";
        /// <inheritdoc />
        public override string SortKeyPrefix => "WORKFLOW#";
    }
}