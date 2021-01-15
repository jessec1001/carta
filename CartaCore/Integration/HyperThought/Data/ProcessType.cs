using System.Runtime.Serialization;

namespace CartaCore.Integration.HyperThought.Data
{
    public enum ProcessType
    {
        [EnumMember(Value = "workflow")]
        Workflow,
        [EnumMember(Value = "decision")]
        Decision,
        [EnumMember(Value = "process")]
        Process
    }
}