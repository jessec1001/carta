using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CartaCore.Extensions.String;
using CartaCore.Operations;

namespace CartaWeb.Models.DocumentItem
{
    /// <summary>
    /// Represents a job associated with an operation execution.
    /// </summary>
    public class JobItem : Item
    {
        /// <summary>
        /// Whether the operation has been completed and a result is available.
        /// </summary>
        public bool Completed { get; set; }
        /// <summary>
        /// The total input values that were specified for the operation.
        /// </summary>
        public Dictionary<string, object> Value { get; set; }
        /// <summary>
        /// The result of the operation. If <see cref="Completed"/> is <c>false</c>, this is <c>null</c>.
        /// </summary>
        public object Result { get; set; }

        /// <summary>
        /// A dictionary of operation identifier keys to operation status values.
        /// Contains information about the status of the operation and suboperations if applicable.
        /// </summary>
        public Dictionary<string, JobItemStatus> Status { get; set; }
        /// <summary>
        /// The authentication objects of the operation. These are stored in key value pairs where:
        /// - The key is the identifier of the operation to authenticate.
        /// - The value is a dictionary of authentication (type, value) pairs.
        /// </summary>
        /// <example>
        /// <code>
        /// Authentication = new()
        /// {
        ///     ["12345678"] = new()
        ///     {
        ///         ["hyperthought"] = new HyperthoughtAuthentication("abcdef"),
        ///         ["mysql"] = new MySqlAuthentication() { Username = "user", Password = "pass" }
        ///     }    
        /// }
        /// </code>
        /// </example>
        [Secret]
        public Dictionary<string, Dictionary<string, object>> Authentication { get; set; }

        /// <summary>
        /// A list of tasks that need to be completed in order for the job to complete. If this list is empty, the
        /// operation is running if not already <see cref="Completed" />.
        /// </summary>
        public List<OperationTask> Tasks { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobItem"/> class.
        /// </summary>
        public JobItem() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="JobItem"/> class with specified identifiers.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="operationId">The operation identifier.</param>
        public JobItem(string jobId, string operationId)
            : base(operationId, jobId) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="JobItem"/> class from the specified job.
        /// </summary>
        /// <param name="job">The operation job.</param>
        public JobItem(OperationJob job)
            : this(job.Id, job.Operation.Id)
        {
            // Copy the status, authentication, and tasks information.
            Status = new Dictionary<string, JobItemStatus>();
            foreach (KeyValuePair<string, OperationStatus> pair in job.Status)
                Status.Add(pair.Key, new JobItemStatus(pair.Value));

            Authentication = new Dictionary<string, Dictionary<string, object>>();
            foreach (KeyValuePair<string, ConcurrentDictionary<string, object>> opAuthPair in job.Authentication)
            {
                var auth = new Dictionary<string, object>();
                foreach (KeyValuePair<string, object> authPair in opAuthPair.Value)
                    auth.Add(authPair.Key, authPair.Value);
                Authentication.Add(opAuthPair.Key, auth);
            }

            Tasks = new List<OperationTask>();
            foreach (OperationTask task in job.Tasks)
                Tasks.Add(task);

            // Copy the output into the result field.
            Result = job.Output;
        }

        /// <inheritdoc />
        [JsonIgnore]
        public override string PartitionKeyPrefix => "OPERATION#";
        /// <inheritdoc />
        [JsonIgnore]
        public override string SortKeyPrefix => "JOB#";
    }
    /// <summary>
    /// Represents the status of an operation for a specific job.
    /// </summary>
    public class JobItemStatus
    {
        /// <summary>
        /// The identifier of the parent operation being executed,
        /// </summary>
        public string ParentId { get; init; }
        /// <summary>
        /// The identifier of the current operation being executed. 
        /// </summary>
        public string OperationId { get; init; }

        /// <summary>
        /// Whether the operation has started execution.
        /// </summary>
        public bool Started { get; init; }
        /// <summary>
        /// Whether the operation has finished execution.
        /// </summary>
        public bool Finished { get; init; }
        /// <summary>
        /// The progress made on the current operation.
        /// Should be in the range [0.0, 1.0].
        /// </summary>
        public double Progress { get; init; }

        /// <summary>
        /// The type of exception that has been thrown if any.
        /// </summary>
        public string ExceptionType { get; init; }
        /// <summary>
        /// The message of the exception that has been thrown if any.
        /// </summary>
        public string ExceptionMessage { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobItemStatus"/> class.
        /// </summary>
        public JobItemStatus() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="JobItemStatus"/> class from a specified status item.
        /// </summary>
        /// <param name="status">The operation status item.</param>
        public JobItemStatus(OperationStatus status)
        {
            ParentId = status.ParentId;
            OperationId = status.OperationId;

            Started = status.Started;
            Finished = status.Finished;
            Progress = status.Progress;

            if (status.Exception is not null)
            {
                ExceptionType = status.Exception.GetType().Name.FromCodified();
                ExceptionMessage = status.Exception.Message;
            }
        }
    }
}