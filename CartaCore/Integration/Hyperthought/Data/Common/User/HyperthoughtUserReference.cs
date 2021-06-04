using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents basic information about a HyperThought user's identity.
    /// </summary>
    public class HyperthoughtUserReference
    {
        /// <summary>
        /// The user's identifier number.
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }
        /// <summary>
        /// The user's unique username.
        /// </summary>
        [JsonPropertyName("username")]
        public string Username { get; set; }
        /// <summary>
        /// The user's full name.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}