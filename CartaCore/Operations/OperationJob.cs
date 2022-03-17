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
        public CancellationToken CancellationToken { get; set; }

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
        /// These are stored for each operation that has had execution initiated.
        /// If an operation needs to update a status, it should modify this dictionary directly and trigger a job update.
        /// </summary>
        public ConcurrentDictionary<string, OperationStatus> Status { get; private init; }
        /// <summary>
        /// Contains authentication objects for each operation stored by key and value.
        /// This is concurrent so that multiple jobs may access authentication entries simultaneously.
        /// </summary>
        public ConcurrentDictionary<string, ConcurrentDictionary<string, object>> Authentication { get; private init; }
        /// <summary>
        /// Contains tasks that must be executed in order for the executing operation to complete.
        /// If an operation needs to initiate a task, it should add to this list and trigger a job update.
        /// </summary>
        public ConcurrentBag<OperationTask> Tasks { get; private init; }

        /// <summary>
        /// Handles updates that are made to the job.
        /// </summary>
        public JobUpdateHandler OnUpdate { get; set; } = (OperationJob job) => Task.CompletedTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationJob"/> class.
        /// </summary>
        /// <param name="operation">The executing operation.</param>
        /// <param name="id">The identifier of the job.</param>
        public OperationJob(
            Operation operation,
            string id)
        : this(operation, id, null) { }
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
        : this(operation, id, parent, default(CancellationToken)) { }
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
        : this(
            operation,
            id,
            new Dictionary<string, object>(),
            new Dictionary<string, object>(),
            parent,
            cancellationToken
        )
        { }
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
        : this(operation, id, input, output, null) { }
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
        : this(operation, id, input, output, parent, default(CancellationToken)) { }
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
            Authentication = new ConcurrentDictionary<string, ConcurrentDictionary<string, object>>();
            Tasks = new ConcurrentBag<OperationTask>();
        }
    }
}