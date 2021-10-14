using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Reference to a member of workspace
    /// </summary>
    public class HyperthoughtWorkspaceMember
    {
        /// <summary>
        /// The user's unique username.
        /// </summary>
        [JsonPropertyName("username")]
        public string Username { get; set; }
        /// <summary>
        /// The user's role (Member/Manager).
        /// </summary>
        [JsonPropertyName("role")]
        public HyperthoughtUserPermissions Role { get; set; }
    }
}