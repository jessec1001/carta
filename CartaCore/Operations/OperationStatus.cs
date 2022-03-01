using System;

namespace CartaCore.Operations
{
    /// <summary>
    /// The status of an operation that has been requested for execution.
    /// </summary>
    public record OperationStatus
    {
        /// <summary>
        /// The identifier of the root operation being executed.
        /// </summary>
        public string RootId { get; init; }
        /// <summary>
        /// The identifier of the parent operation being executed.
        /// </summary>
        public string ParentId { get; init; }
        /// <summary>
        /// The identifier of the operation being executed.
        /// </summary>
        public string OperationId { get; init; }

        /// <summary>
        /// Whether the operation has started execution.
        /// </summary>
        public bool Started { get; set; }
        /// <summary>
        /// Whether the operation has finished execution.
        /// </summary>
        public bool Finished { get; set; }
        /// <summary>
        /// The progress made on the current operation.
        /// Should be in the range [0.0, 1.0].
        /// </summary>
        public double Progress { get; set; }
    
        /// <summary>
        /// An exception that has occurred while executing 
        /// </summary>
        public Exception Exception { get; set; }
    }
}