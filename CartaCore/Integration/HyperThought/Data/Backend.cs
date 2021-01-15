using System.Runtime.Serialization;
using System.Text.Json.Serialization;

using CartaCore.Serialization.Json;

namespace CartaCore.Integration.HyperThought.Data
{
    [JsonConverter(typeof(JsonExactStringEnumConverter))]
    public enum Backend
    {
        [EnumMember(Value = "default")]
        Default,
        [EnumMember(Value = "s3")]
        S3
    }
}