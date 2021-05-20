using System.Runtime.Serialization;
using System.Text.Json.Serialization;

using CartaCore.Serialization.Json;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents the type of backend storing a particular file.
    /// </summary>
    [JsonConverter(typeof(JsonFullStringEnumConverter))]
    public enum HyperthoughtBackend
    {
        /// <summary>
        /// The default data backend.
        /// </summary>
        [EnumMember(Value = "default")]
        Default,
        /// <summary>
        /// The Simple Storage Service (S3) data backend.
        /// </summary>
        [EnumMember(Value = "s3")]
        S3
    }
}