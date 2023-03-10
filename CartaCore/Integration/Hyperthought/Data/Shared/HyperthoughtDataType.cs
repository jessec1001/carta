using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using CartaCore.Serialization.Json;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents information about the type of data in a HyperThought property.
    /// </summary>
    [JsonConverter(typeof(JsonFullStringEnumConverter))]
    public enum HyperthoughtDataType
    {
        /// <summary>
        /// The data is of unknown type.
        /// </summary>
        [EnumMember(Value = "")]
        Unknown,
        /// <summary>
        /// The data is of type integer.
        /// </summary>
        [EnumMember(Value = "integer")]
        Integer,
        /// <summary>
        /// The data is of type floating decimal.
        /// </summary>
        [EnumMember(Value = "decimal")]
        Decimal,
        /// <summary>
        /// The data is of type floating double.
        /// </summary>
        [EnumMember(Value = "double")]
        Double,
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