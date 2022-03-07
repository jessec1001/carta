using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using CartaCore.Serialization.Json;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents the permission level of an individual user.
    /// </summary>
    [JsonConverter(typeof(JsonFullStringEnumConverter))]
    public enum HyperthoughtUserPermissions
    {
        /// <summary>
        /// A value representing that a user is a basic member.
        /// </summary>
        [EnumMember(Value = "Member")]
        Member,
        /// <summary>
        /// A value representing that a user is a manager.
        /// </summary>
        [EnumMember(Value = "Manager")]
        Manager
    }
}