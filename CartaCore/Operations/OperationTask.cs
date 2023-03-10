namespace CartaCore.Operations
{
    // TODO: See if we need to provide different data structures for different types of tasks.
    /// <summary>
    /// The type of task that needs to be completed for an operation.
    /// </summary>
    public enum OperationTaskType
    {
        /// <summary>
        /// A file needs to be uploaded to the operation.
        /// </summary>
        File,
        /// <summary>
        /// A user needs to be authenticated for an operation.
        /// </summary>
        Authenticate,
    }

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
        /// The unique operation that the task is for.
        /// </summary>
        public string Operation { get; set; }
        /// <summary>
        /// The unique field that the task refers to.
        /// </summary>
        public string Field { get; set; }
    }
}