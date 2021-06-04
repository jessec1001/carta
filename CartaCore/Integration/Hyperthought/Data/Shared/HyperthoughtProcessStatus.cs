using System.Runtime.Serialization;
using System.Text.Json.Serialization;

using CartaCore.Serialization.Json;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents the status of a particular HyperThought workflow.
    /// </summary>
    [JsonConverter(typeof(JsonFullStringEnumConverter))]
    public enum HyperthoughtProcessStatus
    {
        /// <summary>
        /// A value representing that no status has been set.
        /// </summary>
        [EnumMember(Value = "")]
        None,
        /// <summary>
        /// A value representing that the workflow is pending execution.
        /// </summary>
        [EnumMember(Value = "pending")]
        Pending,
        /// <summary>
        /// A value representing that the workflow is in progress.
        /// </summary>
        [EnumMember(Value = "in progress")]
        InProgress,
        /// <summary>
        /// A value representing that the workflow is awaiting an assignee.
        /// </summary>
        [EnumMember(Value = "awaiting assignee")]
        AwaitingAssignee,
        /// <summary>
        /// A value representing that the workflow is pending manager review.
        /// </summary>
        [EnumMember(Value = "manager review")]
        ManagerReview,
        /// <summary>
        /// A value representing that the workflow has been rejected.
        /// </summary>
        [EnumMember(Value = "rejected")]
        Rejected,
        /// <summary>
        /// A value representing that the workflow has been completed.
        /// </summary>
        [EnumMember(Value = "completed")]
        Completed,
        /// <summary>
        /// A value representing that the workflow has been completely designed.
        /// </summary>
        [EnumMember(Value = "workflow complete")]
        WorkflowComplete,
        /// <summary>
        /// A value representing that the workflow will not be executed.
        /// </summary>
        [EnumMember(Value = "will not execute")]
        WillNotExecute
    }
}