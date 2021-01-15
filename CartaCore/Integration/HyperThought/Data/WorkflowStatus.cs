using System.Runtime.Serialization;

namespace CartaCore.Integration.HyperThought.Data
{
    public enum WorkflowStatus
    {
        [EnumMember(Value = "")]
        None,
        [EnumMember(Value = "pending")]
        Pending,
        [EnumMember(Value = "in progress")]
        InProgress,
        [EnumMember(Value = "awaiting assignee")]
        AwaitingAssignee,
        [EnumMember(Value = "manager review")]
        ManagerReview,
        [EnumMember(Value = "rejected")]
        Rejected,
        [EnumMember(Value = "completed")]
        Completed,
        [EnumMember(Value = "workflow complete")]
        WorkflowComplete,
        [EnumMember(Value = "will not execute")]
        WillNotExecute
    }
}