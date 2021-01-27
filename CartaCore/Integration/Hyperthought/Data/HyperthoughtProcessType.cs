using System.Runtime.Serialization;
using System.Text.Json.Serialization;

using CartaCore.Serialization.Json;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// The type of process of a HyperThought node.
    /// </summary>
    [JsonConverter(typeof(JsonExactStringEnumConverter))]
    public enum HyperthoughtProcessType
    {
        /// <summary>
        /// A workflow.
        /// </summary>
        [EnumMember(Value = "workflow")]
        Workflow,
        /// <summary>
        /// A decision.
        /// </summary>
        [EnumMember(Value = "decision")]
        Decision,
        /// <summary>
        /// A process.
        /// </summary>
        [EnumMember(Value = "process")]
        Process
    }
}