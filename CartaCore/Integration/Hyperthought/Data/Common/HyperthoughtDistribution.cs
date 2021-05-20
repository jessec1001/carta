using System.Runtime.Serialization;
using System.Text.Json.Serialization;

using CartaCore.Serialization.Json;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents the distribution version that an object is restricted to.
    /// </summary>
    [JsonConverter(typeof(JsonFullStringEnumConverter))]
    public enum HyperthoughtDistribution
    {
        /// <summary>
        /// No specific distribution.
        /// </summary>
        [EnumMember(Value = "")]
        None,
        /// <summary>
        /// Distribution A.
        /// </summary>
        [EnumMember(Value = "distribution a")]
        DistributionA,
        /// <summary>
        /// Distribution B.
        /// </summary>
        [EnumMember(Value = "distribution b")]
        DistributionB,
        /// <summary>
        /// Distribution C.
        /// </summary>
        [EnumMember(Value = "distribution c")]
        DistributionC,
        /// <summary>
        /// Distribution D.
        /// </summary>
        [EnumMember(Value = "distribution d")]
        DistributionD,
        /// <summary>
        /// Distribution E.
        /// </summary>
        [EnumMember(Value = "distribution e")]
        DistributionE,
        /// <summary>
        /// Distribution F.
        /// </summary>
        [EnumMember(Value = "distribution f")]
        DistributionF
    }
}