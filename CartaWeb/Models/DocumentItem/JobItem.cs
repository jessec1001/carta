using System.Collections.Generic;
using System.Text.Json.Serialization;
using CartaCore.Operations;

namespace CartaWeb.Models.DocumentItem
{
    /// <summary>
    /// Represents a job associated with an operation execution.
    /// </summary>
    public class JobItem : Item
    {
        // TODO: Implement operation status here.
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

        /// <inheritdoc />
        [JsonIgnore]
        public override string PartitionKeyPrefix => "OPERATION#";
        /// <inheritdoc />
        [JsonIgnore]
        public override string SortKeyPrefix => "JOB#";
    }
}