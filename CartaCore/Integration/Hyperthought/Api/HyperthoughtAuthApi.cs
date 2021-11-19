using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using CartaCore.Integration.Hyperthought.Data;

namespace CartaCore.Integration.Hyperthought.Api
{
    /// <summary>
    /// Represents the functionality of the Auth and OIDC modules of the HyperThought API.
    /// </summary>
    public class HyperthoughtAuthApi
    {
        private HyperthoughtApi Api { get; init; }

        /// <summary>
        /// Initializes an instance of the <see cref="HyperthoughtAuthApi"/> class with the specified base API.
        /// </summary>
        /// <param name="api"></param>
        public HyperthoughtAuthApi(HyperthoughtApi api)
        {
            Api = api;
        }

        /// <summary>
        /// Gets the auth API URI at the HyperThought instance.
        /// </summary>
        protected Uri GetApiUri() => new Uri(Api.GetBaseUri(), "api/");
        /// <summary>
        /// Gets the OIDC URI at the HyperThought instance.
        /// </summary>
        protected Uri GetOidcUri() => new Uri(new Uri(Api.Access?.BaseUrl), "openid/");

        /// <summary>
        /// Obtains the user information of the currently authenticated HyperThought user.
        /// </summary>
        /// <returns>The user information obtained from the HyperThought API.</returns>
        public async Task<HyperthoughtUserInfo> GetUserInfoAsync()
        {
            Uri requestUri = new Uri(Api.GetBaseUri(), "auth/userinfo/");
            return await Api.GetJsonObjectAsync<HyperthoughtUserInfo>(requestUri);
        }

        /// <summary>
        /// Refreshes the HyperThought API access information for the currently authenticated HyperThought user.
        /// </summary>
        /// <returns>Nothing.</returns>
        public async Task RefreshAccess()
        {
            // Construct the request DTO.
            HyperthoughtPostRefreshRequest request = new HyperthoughtPostRefreshRequest(Api.Access);

            // Grab the new access information.
            Uri requestUri = new Uri(GetOidcUri(), "token/");
            HyperthoughtPostRefreshResponse response = await Api
                .PostJsonObjectAsync<HyperthoughtPostRefreshRequest, HyperthoughtPostRefreshResponse>(requestUri, request);

            // Update the API access from the response DTO.
            HyperthoughtApiAccess oldAccess = Api.Access;
            HyperthoughtApiAccess newAccess = new HyperthoughtApiAccess
            {
                AccessToken = response.AccessToken,
                RefreshToken = response.RefreshToken,

                ExpirationInterval = response.ExpirationInterval,
                ExpirationTime = DateTime.Now.Add(TimeSpan.FromSeconds(response.ExpirationInterval)),

                BaseUrl = oldAccess.BaseUrl,
                ClientId = oldAccess.ClientId,
                ClientSecret = oldAccess.ClientSecret
            };
            Api.Access = newAccess;
        }

        /// <summary>
        /// Represents a data-transfer object sent to the refresh token OIDC endpoint.
        /// </summary>
        private class HyperthoughtPostRefreshRequest
        {
            /// <summary>
            /// The user client ID.
            /// </summary>
            [JsonPropertyName("client_id")]
            public string ClientId { get; set; }
            /// <summary>
            /// The user client secret.
            /// </summary>
            [JsonPropertyName("client_secret")]
            public string ClientSecret { get; set; }
            /// <summary>
            /// The type of grant being used to generate a new access token.
            /// </summary>
            [JsonPropertyName("grant_type")]
            public string GrantType { get; set; } = "refresh_token";
            /// <summary>
            /// The token used to refresh the API access key.
            /// </summary>
            [JsonPropertyName("refresh_token")]
            public string RefreshToken { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="HyperthoughtPostRefreshRequest"/> class from the specified
            /// HyperThought API access.
            /// </summary>
            /// <param name="access">The existing HyperThought API access.</param>
            public HyperthoughtPostRefreshRequest(HyperthoughtApiAccess access)
            {
                ClientId = access.ClientId;
                ClientSecret = access.ClientSecret;

                RefreshToken = access.RefreshToken;
            }
        }
        /// <summary>
        /// Represents a data-transfer object received from the refresh token OIDC endpoint.
        /// </summary>
        private class HyperthoughtPostRefreshResponse
        {
            /// <summary>
            /// The token used to access HyperThought resources.
            /// </summary>
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; }
            /// <summary>
            /// The token used to refresh the API access key.
            /// </summary>
            [JsonPropertyName("refresh_token")]
            public string RefreshToken { get; set; }
            /// <summary>
            /// The token used to identify user information.
            /// </summary>
            [JsonPropertyName("id_token")]
            public string IdToken { get; set; }

            /// <summary>
            /// The type of authentication token.
            /// </summary>
            [JsonPropertyName("token_type")]
            public string TokenType { get; set; }

            /// <summary>
            /// The number of seconds until the access key expires.
            /// </summary>
            [JsonPropertyName("expires_in")]
            public int ExpirationInterval { get; set; }
        }
    }
}