using System;

namespace CartaCore.Operations
{
    /// <summary>
    /// Represents an operation and input/output key that a connection can connect to as a source or target.
    /// </summary>
    public class WorkflowOperationConnectionPoint
    {
        /// <summary>
        /// The unique identifier of the operation to connect to.
        /// </summary>
        public string Operation { get; init; }
        /// <summary>
        /// The name of the input/output key to connect to.
        /// </summary>
        public string Field { get; init; }
    }

    /// <summary>
    /// A connection between a pair of operation nodes within a workflow operation. Specifies that an output from a
    /// source operation should be provided as an input to a target operation.
    /// </summary>
    public class WorkflowOperationConnection
    {
        /// <summary>
        /// The source or input of the workflow connection.
        /// </summary>
        public WorkflowOperationConnectionPoint Source { get; init; }
        /// <summary>
        /// The target or output of the workflow connection.
        /// </summary>
        public WorkflowOperationConnectionPoint Target { get; init; }

        // TODO: Temporary to be replaced by `Multiplicity`.
        /// <summary>
        /// Whether this connector is a multiplexing connector or not. If the connector is a multiplexer, the source is
        /// assumed to be of type `T[]` and the target is assumed to be of type `T`. This also implies that any output
        /// connectors attached to an operation with a multiplexer input will have output values of type `U` turned into
        /// values of type `U[]`.
        /// </summary>
        public bool Multiplexer { get; init; }

        // TODO: Currently unused.
        public string Property { get; init; }

        // TODO: Currently unused.
        public int[] Multiplicity { get; init; }
    }
}