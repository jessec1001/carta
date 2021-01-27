using System.Runtime.Serialization;
using System.Text.Json.Serialization;

using CartaCore.Serialization.Json;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents the instance of the HyperThought export.
    /// </summary>
    [JsonConverter(typeof(JsonExactStringEnumConverter))]
    public enum HyperthoughtExportControl
    {
        /// <summary>
        /// No particular export.
        /// </summary>
        [EnumMember(Value = "")]
        None,
        /// <summary>
        /// International Traffic in Arms Regulations (ITAR).
        /// </summary>
        [EnumMember(Value = "itar")]
        ITAR,
        /// <summary>
        /// Export Administration Regulations (EAR).
        /// </summary>
        [EnumMember(Value = "ear")]
        EAR
    }
}