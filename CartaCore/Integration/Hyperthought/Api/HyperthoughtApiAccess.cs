using System;
using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Api
{
    /// <summary>
    /// Represents the fields stored in a HyperThought API access key.
    /// </summary>
    public class HyperthoughtApiAccess
    {
        /// <summary>
        /// The token used to access HyperThought resources.
        /// </summary>
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }
        /// <summary>
        /// The token used to refresh the API access key.
        /// </summary>
        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// The number of seconds until the access key expires.
        /// </summary>
        [JsonPropertyName("expiresIn")]
        public int ExpirationInterval { get; set; }
        /// <summary>
        /// The time at which the access key expires.
        /// </summary>
        [JsonPropertyName("expiresAt")]
        public DateTime ExpirationTime { get; set; }

        /// <summary>
        /// The base URL for the HyperThought instance.
        /// </summary>
        [JsonPropertyName("baseUrl")]
        public string BaseUrl { get; set; }
        /// <summary>
        /// The user client ID.
        /// </summary>
        [JsonPropertyName("clientId")]
        public string ClientId { get; set; }
        /// <summary>
        /// The user client secret.
        /// </summary>
        [JsonPropertyName("clientSecret")]
        public string ClientSecret { get; set; }
    }
}