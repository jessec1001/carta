using System.Runtime.Serialization;
using System.Text.Json.Serialization;

using CartaCore.Serialization.Json;

namespace CartaCore.Integration.HyperThought.Data
{
    [JsonConverter(typeof(JsonExactStringEnumConverter))]
    public enum Distribution
    {
        [EnumMember(Value = "distribution a")]
        DistributionA,
        [EnumMember(Value = "distribution b")]
        DistributionB,
        [EnumMember(Value = "distribution c")]
        DistributionC,
        [EnumMember(Value = "distribution d")]
        DistributionD,
        [EnumMember(Value = "distribution e")]
        DistributionE,
        [EnumMember(Value = "distribution f")]
        DistributionF
    }
}