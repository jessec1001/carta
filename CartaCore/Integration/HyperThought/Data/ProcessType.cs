using System.Runtime.Serialization;
using System.Text.Json.Serialization;

using CartaCore.Serialization.Json;

namespace CartaCore.Integration.HyperThought.Data
{
    [JsonConverter(typeof(JsonExactStringTEnumConverter))]
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