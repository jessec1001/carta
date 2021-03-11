using System.Runtime.Serialization;
using System.Text.Json.Serialization;

using CartaCore.Serialization.Json;

namespace CartaWeb.Models.Meta
{
    /// <summary>
    /// Represents the format that a parameter can be retrieved from.
    /// </summary>
    [JsonConverter(typeof(FullStringEnumConverter))]
    public enum ApiParameterFormat
    {
        /// <summary>The parameter was retrieved from route.</summary>
        [EnumMember(Value = "route")]
        Route,
        /// <summary>The parameter was retrieved from query.</summary>
        [EnumMember(Value = "query")]
        Query
    }
}