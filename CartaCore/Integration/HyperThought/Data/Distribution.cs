using System.Runtime.Serialization;

namespace CartaCore.Integration.HyperThought.Data
{
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