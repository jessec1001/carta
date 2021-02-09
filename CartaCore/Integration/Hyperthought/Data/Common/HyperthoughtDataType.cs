using System.Runtime.Serialization;
using System.Text.Json.Serialization;

using CartaCore.Serialization.Json;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents information about the type of data in a HyperThought property.
    /// </summary>
    [JsonConverter(typeof(FullStringEnumConverter))]
    public enum HyperthoughtDataType
    {
        /// <summary>
        /// The data is of type string.
        /// </summary>
        [EnumMember(Value = "string")]
        String,
        /// <summary>
        /// The data is a of a reference type.
        /// </summary>
        [EnumMember(Value = "link")]
        Link
    }
}