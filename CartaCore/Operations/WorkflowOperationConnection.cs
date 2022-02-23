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

        /*
            The following attributes of the connection point can be applied in synergy.

            For instance, `Index` can be specified on the source and `Multiplicity` can be defined on the target to
            execute the target operation on each of the indexed values from the source value.
        */

        /// <summary>
        /// If specified, should be used to reference a particular index of the value passed across this connection.
        /// For instance, if the value is a graph, setting this value to "foo" will reference the enumeration of values
        /// on vertices corresponding to the "foo" property.
        /// - When specified on the source, this will convert from a structure to a list of indexed values.
        /// - When specified on the target, this will convert from a list of indexed values to a structure.
        /// </summary>
        public string Index { get; init; }
        /// <summary>
        /// If specified, should be a tuple of the form (index, multiplicity) with the following definitions:
        /// - The index is used to reshape the source value into a target value based on corresponding multiplicity
        ///   along the connection. For instance, if source has multiplicity [(1, null), (0, null)] and target has
        ///   multiplicity [(0, null), (1, null)], then the MxN source value is reshaped to a NxM target value.
        /// - The multiplicity is either null or an integer. If null, the number of elements in the source value is used
        ///   to determine the number of elements in the target value. Otherwise, the number of elements is specified by
        ///   the multiplicity. Thus, multiplicity can only be null if it is null on source and target (based on index).
        /// </summary>
        public (int, int?)[] Multiplicity { get; init; }
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
    }
}