using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CartaCore.Graphs;

namespace CartaCore.Operations
{
    // TODO: Provide authentication details.
    // TODO: Provide an `ILogger` in the context that allows for logging to be performed (if not null).

    /// <summary>
    /// Represents a function delegate that handles a job update.
    /// </summary>
    /// <param name="job">The job being updated.</param>
    /// <returns>Nothing</returns>
    public delegate Task JobUpdateHandler(OperationJob job);

    /// <summary>
    /// Represents the execution information of a particular operation.
    /// </summary>
    public class OperationJob
    {
        /// <summary>
        /// The unique identifier of this job.
        /// </summary>
        public string Id { get; private init; }
        /// <summary>
        /// The operation that is being executed.
        /// </summary>
        public Operation Operation { get; private init; }

        /// <summary>
        /// A cancellation token that can be used to cancel the operation.
        /// </summary>
        public CancellationToken CancellationToken { get; private init; }

        /// <summary>
        /// The parent job of this job.
        /// </summary>
        public OperationJob Parent { get; private init; }
        /// <summary>
        /// The root job of this job.
        /// </summary>
        public OperationJob Root
        {
            get
            {
                OperationJob root = this;
                while (root.Parent is not null)
                    root = root.Parent;
                return root;
            }
        }

        /// <summary>
        /// Contains information about selections on fields that should be prioritized.
        /// The keys represent the field name that should be prioritized.
        /// The values are priority queues storing a list of selector/parameter pairs of the form
        /// <see cref="ISelector{TSource, TTarget}" />.
        /// The selectors are left to be interpreted by relevant operations.
        /// </summary>
        /// <value></value>
        public ConcurrentDictionary<string, ConcurrentQueue<(object, object)>> PriorityQueue { get; private init; }

        /// <summary>
        /// The input fields for the executing operation.
        /// </summary>
        public ConcurrentDictionary<string, object> Input { get; private init; }
        /// <summary>
        /// The output fields for the executing operation.
        /// </summary>
        public ConcurrentDictionary<string, object> Output { get; private init; }

        /// <summary>
        /// The status for each operation that is executed as a result of this job.
        /// </summary>
        public ConcurrentDictionary<string, OperationStatus> Status { get; private init; }
        // TODO: This should be generalized to support all types of tasks.
        // TODO: We really need to construct a generalized task structure.
        /// <summary>
        /// Contains authentication objects for each operation stored by key and value.
        /// </summary>
        public ConcurrentDictionary<string, ConcurrentDictionary<string, object>> Authentication { get; private init; }

        /// <summary>
        /// Handles updates that are made to the job.
        /// </summary>
        public JobUpdateHandler OnUpdate { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationJob"/> class.
        /// </summary>
        /// <param name="operation">The executing operation.</param>
        /// <param name="id">The identifier of the job.</param>
        public OperationJob(
            Operation operation,
            string id)
        {
            Operation = operation;
            Id = id;
            Input = new ConcurrentDictionary<string, object>();
            Output = new ConcurrentDictionary<string, object>();
            Status = new ConcurrentDictionary<string, OperationStatus>();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationJob"/> class.
        /// </summary>
        /// <param name="operation">The executing operation.</param>
        /// <param name="id">The identifier of the job.</param>
        /// <param name="parent">The parent job.</param>
        public OperationJob(
            Operation operation,
            string id,
            OperationJob parent)
        {
            Operation = operation;
            Id = id;
            Parent = parent;
            Input = new ConcurrentDictionary<string, object>();
            Output = new ConcurrentDictionary<string, object>();
            Status = new ConcurrentDictionary<string, OperationStatus>();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationJob"/> class.
        /// </summary>
        /// <param name="operation">The executing operation.</param>
        /// <param name="id">The identifier of the job.</param>
        /// <param name="parent">The parent job.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        public OperationJob(
            Operation operation,
            string id,
            OperationJob parent,
            CancellationToken cancellationToken)
        {
            Operation = operation;
            Id = id;
            Parent = parent;
            CancellationToken = cancellationToken;
            Input = new ConcurrentDictionary<string, object>();
            Output = new ConcurrentDictionary<string, object>();
            Status = new ConcurrentDictionary<string, OperationStatus>();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationJob"/> class.
        /// </summary>
        /// <param name="operation">The executing operation.</param>
        /// <param name="id">The identifier of the job.</param>
        /// <param name="input">The input to the operation.</param>
        /// <param name="output">The output to the operation.</param>
        public OperationJob(
            Operation operation,
            string id,
            Dictionary<string, object> input,
            Dictionary<string, object> output)
        {
            Operation = operation;
            Id = id;
            Input = new ConcurrentDictionary<string, object>(input);
            Output = new ConcurrentDictionary<string, object>(output);
            Status = new ConcurrentDictionary<string, OperationStatus>();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationJob"/> class.
        /// </summary>
        /// <param name="operation">The executing operation.</param>
        /// <param name="id">The identifier of the job.</param>
        /// <param name="input">The input to the operation.</param>
        /// <param name="output">The output to the operation.</param>
        /// <param name="parent">The parent job.</param>
        public OperationJob(
            Operation operation,
            string id,
            Dictionary<string, object> input,
            Dictionary<string, object> output,
            OperationJob parent)
        {
            Operation = operation;
            Id = id;
            Parent = parent;
            Input = new ConcurrentDictionary<string, object>(input);
            Output = new ConcurrentDictionary<string, object>(output);
            Status = new ConcurrentDictionary<string, OperationStatus>();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationJob"/> class.
        /// </summary>
        /// <param name="operation">The executing operation.</param>
        /// <param name="id">The identifier of the job.</param>
        /// <param name="input">The input to the operation.</param>
        /// <param name="output">The output to the operation.</param>
        /// <param name="parent">The parent job.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        public OperationJob(
            Operation operation,
            string id,
            Dictionary<string, object> input,
            Dictionary<string, object> output,
            OperationJob parent,
            CancellationToken cancellationToken)
        {
            Operation = operation;
            Id = id;
            Parent = parent;
            CancellationToken = cancellationToken;
            Input = new ConcurrentDictionary<string, object>(input);
            Output = new ConcurrentDictionary<string, object>(output);
            Status = new ConcurrentDictionary<string, OperationStatus>();
        }
    }

    // TODO: Remove old context code.
    // /// <summary>
    // /// A context that contains execution information for an operation.
    // /// </summary>
    // public class OperationContextOld
    // {
    //     // TODO: We need to be able to hash and compare selectors so that they are not duplicated on the queue.
    //     // Create a queue to store selectors based on priority.
    //     public ConcurrentQueue<(Selector, object)> Selectors { get; } = new();

    //     /// <summary>
    //     /// The tasks for the operation. 
    //     /// </summary>
    //     public ConcurrentBag<OperationTask> Tasks { get; } = new();

    //     public OperationFileSaveHandler SaveFile;
    //     public OperationFileLoadHandler LoadFile;
    //     public OperationJobUpdateHandler UpdateJob;
    // }
}