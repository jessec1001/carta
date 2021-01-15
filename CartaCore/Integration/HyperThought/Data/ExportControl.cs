using System.Runtime.Serialization;
using System.Text.Json.Serialization;

using CartaCore.Serialization.Json;

namespace CartaCore.Integration.HyperThought.Data
{
    [JsonConverter(typeof(JsonExactStringTEnumConverter))]
    public enum ExportControl
    {
        [EnumMember(Value = "itar")]
        ITAR,
        [EnumMember(Value = "ear")]
        EAR
    }
}