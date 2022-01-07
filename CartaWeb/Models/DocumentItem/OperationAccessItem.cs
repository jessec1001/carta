using System.Text.Json.Serialization;
using CartaWeb.Models.Data;

namespace CartaWeb.Models.DocumentItem
{
    /// <summary>
    /// Used to store information about an operation that is accessible via a workspace.
    /// </summary>
    public class OperationAccessItem : Item
    {
        /// <summary>
        /// The history of access to the referenced operation.
        /// </summary>
        public DocumentHistory DocumentHistory { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationAccessItem"/> class.
        /// </summary>
        public OperationAccessItem() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationAccessItem"/> class referencing a specific operation
        /// in a specific workspace.
        /// </summary>
        /// <param name="workspaceId">The unique identifier of the workspace.</param>
        /// <param name="operationId">The unique identifier of the operation.</param>
        public OperationAccessItem(string workspaceId, string operationId)
            : base(workspaceId, operationId) { }

        /// <inheritdoc />
        [JsonIgnore]
        public override string PartitionKeyPrefix => "WORKSPACE#";
        /// <inheritdoc />
        [JsonIgnore]
        public override string SortKeyPrefix => "OPERATIONACCESS#";
    }

}