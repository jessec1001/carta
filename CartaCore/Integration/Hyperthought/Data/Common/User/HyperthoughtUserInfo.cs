using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents detailed information about a HyperThought user's account.
    /// </summary>
    public class HyperthoughtUserInfo
    {
        /// <summary>
        /// The user's username.
        /// </summary>
        [JsonPropertyName("username")]
        public string Username { get; set; }
        /// <summary>
        /// The user's last name,
        /// </summary>
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }
        /// <summary>
        /// The user's first name.
        /// </summary>
        [JsonPropertyName("last_name")]
        public string LastName { get; set; }
        /// <summary>
        /// The user's email address.
        /// </summary>
        [JsonPropertyName("email")]
        public string Email { get; set; }
    }
}