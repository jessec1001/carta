namespace CartaCore.Operations
{
    /// <summary>
    /// The type of task that needs to be completed for an operation.
    /// </summary>
    public enum OperationTaskType
    {
        /// <summary>
        /// A file needs to be uploaded to the operation.
        /// </summary>
        File
    }

    // TODO: We will need to rework this to work with operations that exist mid-workflow.
    /// <summary>
    /// Represents a task that needs to be performed for an operation before it can continue to execute. 
    /// </summary>
    public class OperationTask
    {
        /// <summary>
        /// The type of task that needs to be completed.
        /// </summary>
        public OperationTaskType Type { get; set; }

        /// <summary>
        /// The field that the task refers to.
        /// </summary>
        public string Field { get; set; }
    }
}