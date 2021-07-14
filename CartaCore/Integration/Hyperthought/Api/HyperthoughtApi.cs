using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

using CartaCore.Serialization.Json;

namespace CartaCore.Integration.Hyperthought.Api
{
    /// <summary>
    /// A simple connector API used to connect Carta to HyperThought.
    /// </summary>
    public class HyperthoughtApi
    {
        private HttpClientHandler ClientHandler;
        private HttpClient Client;
        private HyperthoughtApiAccess ApiAccess;

        private readonly JsonSerializerOptions JsonOptions;

        /// <summary>
        /// Gets or sets the HyperThought API access.
        /// </summary>
        /// <value>The HyperThought API access parameters.</value>
        public HyperthoughtApiAccess Access
        {
            get => ApiAccess;
            set
            {
                // Set the access.
                ApiAccess = value;

                // Set the authorization header.
                Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Access?.AccessToken);
            }
        }

        /// <summary>
        /// Gets or sets the retry attempts amount.
        /// </summary>
        /// <value>The number of times to retry attempting to access a resource.</value>
        public int RetryAttempts { get; set; } = 3;

        /// <summary>
        /// Gets the HyperThought auth API.
        /// </summary>
        /// <value>The HyperThought auth API.</value>
        public HyperthoughtAuthApi Auth { get; private init; }
        /// <summary>
        /// Gets the HyperThought common API.
        /// </summary>
        /// <value>The HyperThought common API.</value>
        public HyperthoughtCommonApi Common { get; private init; }
        /// <summary>
        /// Gets the HyperThought projects API.
        /// </summary>
        /// <value>The HyperThought projects API.</value>
        public HyperthoughtProjectsApi Projects { get; private init; }
        /// <summary>
        /// Gets the HyperThought groups API.
        /// </summary>
        /// <value>The HyperThought groups API.</value>
        public HyperthoughtGroupsApi Groups { get; private init; }
        /// <summary>
        /// Gets the HyperThought files API.
        /// </summary>
        /// <value>The HyperThought files API.</value>
        public HyperthoughtFilesApi Files { get; private init; }
        /// <summary>
        /// Gets the HyperThought workflow API.
        /// </summary>
        /// <value>The HyperThought workflow API.</value>
        public HyperthoughtWorkflowApi Workflow { get; private init; }

        /// <summary>
        /// The base URL from which the HyperThought instance is running.
        /// </summary>
        public Uri GetBaseUri() => new Uri(new Uri(Access?.BaseUrl), "api/");

        /// <summary>
        /// Constructs an instance of the HyperThought API with specified access properties.
        /// </summary>
        /// <param name="access">HyperThought access properties.</param>
        public HyperthoughtApi(HyperthoughtApiAccess access)
        {
            // Set the access properties.
            ApiAccess = access;

            // Create the HTTP client.
            ClientHandler = new HttpClientHandler();
            Client = new HttpClient(ClientHandler);

            // Set the DOD cookie.
            CookieContainer cookies = new CookieContainer();
            cookies.Add(new Cookie("dodAccessBanner", "true") { Domain = GetBaseUri().Host });
            ClientHandler.CookieContainer = cookies;
            ClientHandler.AllowAutoRedirect = true;

            // Set the authorization header.
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Access?.AccessToken);

            // Set the JSON options.
            JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            JsonOptions.Converters.Insert(0, new JsonNullEmptyStringConverter());

            // Create the sub-APIs.
            Auth = new HyperthoughtAuthApi(this);
            Common = new HyperthoughtCommonApi(this);
            Projects = new HyperthoughtProjectsApi(this);
            Groups = new HyperthoughtGroupsApi(this);
            Files = new HyperthoughtFilesApi(this);
            Workflow = new HyperthoughtWorkflowApi(this);
        }
        /// <summary>
        /// Constructs an instance of the HyperThought API with a given API access key.
        /// </summary>
        /// <param name="accessKey">The API access key.</param>
        public HyperthoughtApi(string accessKey)
            : this(ParseAccessKey(accessKey)) { }

        /// <summary>
        /// Reads an object from a specified URI and tries to convert it to a JSON object. Throws an HTTP error if one
        /// has occurred. Will attempt to run multiple times in case of HTTP error.
        /// </summary>
        /// <param name="uri">The URI to retrieve data from.</param>
        /// <param name="attempts">
        /// The number of attempts to use when retrieving data. If not specified, will default to
        /// <see cref="RetryAttempts"/>. Will only reattempt on HTTP errors.
        /// </param>
        /// <typeparam name="T">The type of object to deserialize from JSON.</typeparam>
        /// <returns>The deserialized object returned from the endpoint.</returns>
        public async Task<T> GetJsonObjectAsync<T>(Uri uri, int? attempts = null)
        {
            // Try to make the request multiple times if HTTP errors occur.
            if (!attempts.HasValue) attempts = RetryAttempts;
            try
            {
                HttpResponseMessage response = await Client.GetAsync(uri);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
            }
            catch (HttpRequestException)
            {
                if (--attempts <= 0) throw;
                else return await GetJsonObjectAsync<T>(uri, attempts);
            }
        }
        /// <summary>
        /// Deletes a resource at a specified URI. Throws an HTTP error if one has occurred. Will attempt to run
        /// multiple times in case of HTTP error.
        /// </summary>
        /// <param name="uri">The URI to delete data at.</param>
        /// <param name="attempts">
        /// The number of attempts to use when retrieving data. If not specified, will default to
        /// <see cref="RetryAttempts"/>. Will only reattempt on HTTP errors.
        /// </param>
        /// <returns>Nothing.</returns>
        public async Task DeleteAsync(Uri uri, int? attempts = null)
        {
            // Try to make the request multiple times if HTTP errors occur.
            if (!attempts.HasValue) attempts = RetryAttempts;
            try
            {
                HttpResponseMessage response = await Client.DeleteAsync(uri);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                if (--attempts <= 0) throw;
                else await DeleteAsync(uri, attempts);
            }
        }
        /// <summary>
        /// Deletes a resource at a specified URI by sending a JSON object. Throws an HTTP error if one has occurred.
        /// Will attempt to run multiple times in case of HTTP error.
        /// </summary>
        /// <param name="uri">The URI to delete data at.</param>
        /// <param name="value">The value of the data</param>
        /// <param name="attempts">
        /// The number of attempts to use when retrieving data. If not specified, will default to
        /// <see cref="RetryAttempts"/>. Will only reattempt on HTTP errors.
        /// </param>
        /// <typeparam name="T">The type of object to serialize to request JSON.</typeparam>
        /// <returns>Nothing.</returns>
        public async Task DeleteAsync<T>(Uri uri, T value, int? attempts = null)
        {
            // Try to make the request multiple times if HTTP errors occur.
            if (!attempts.HasValue) attempts = RetryAttempts;
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri)
                {
                    Content = JsonContent.Create<T>(value, options: JsonOptions)
                };
                HttpResponseMessage response = await Client.SendAsync(request);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                if (--attempts <= 0) throw;
                else await DeleteAsync(uri, attempts);
            }
        }
        /// <summary>
        /// Patches an object to a specified URI by first converting to a JSON object. Throws an HTTP error if one has
        /// occurred. Will attempt to run multiple times in case of HTTP error.
        /// </summary>
        /// <param name="uri">The URI to patch data to.</param>
        /// <param name="value">The value of the data.</param>
        /// <param name="attempts">
        /// The number of attempts to use when retrieving data. If not specified, will default to
        /// <see cref="RetryAttempts"/>. Will only reattempt on HTTP errors.
        /// </param>
        /// <typeparam name="T">The type of object to serialize to request JSON.</typeparam>
        /// <typeparam name="U">The type of object to deserialize from response JSON.</typeparam>
        /// <returns>The deserialized object returned from the endpoint.</returns>
        public async Task<U> PatchJsonObjectAsync<T, U>(Uri uri, T value, int? attempts = null)
        {
            // Try to make the request multiple times if HTTP errors occur.
            if (!attempts.HasValue) attempts = RetryAttempts;
            try
            {
                HttpContent content = JsonContent.Create<T>(value, options: JsonOptions);
                HttpResponseMessage response = await Client.PatchAsync(uri, content);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<U>(options: JsonOptions);
            }
            catch (HttpRequestException)
            {
                if (--attempts <= 0) throw;
                else return await PatchJsonObjectAsync<T, U>(uri, value, attempts);
            }
        }
        /// <summary>
        /// Patches an object to a specified URI by first converting to a JSON object. Throws an HTTP error if one has
        /// occurred. Will attempt to run multiple times in case of HTTP error.
        /// </summary>
        /// <param name="uri">The URI to patch data to.</param>
        /// <param name="value">The value of the data.</param>
        /// <param name="attempts">
        /// The number of attempts to use when retrieving data. If not specified, will default to
        /// <see cref="RetryAttempts"/>. Will only reattempt on HTTP errors.
        /// </param>
        /// <typeparam name="T">The type of object to serialize to request JSON.</typeparam>
        /// <returns>Nothing.</returns>
        public async Task PatchJsonObjectAsync<T>(Uri uri, T value, int? attempts = null)
        {
            // Try to make the request multiple times if HTTP errors occur.
            if (!attempts.HasValue) attempts = RetryAttempts;
            try
            {
                HttpContent content = JsonContent.Create<T>(value, options: JsonOptions);
                HttpResponseMessage response = await Client.PatchAsync(uri, content);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                if (--attempts <= 0) throw;
                else await PatchJsonObjectAsync<T>(uri, value, attempts);
            }
        }
        /// <summary>
        /// Posts an object to a specified URI by first converting to a JSON object. Throws an HTTP error if one has
        /// occurred. Will attempt to run multiple times in case of HTTP error.
        /// </summary>
        /// <param name="uri">The URI to post data to.</param>
        /// <param name="value">The value of the data.</param>
        /// <param name="attempts">
        /// The number of attempts to use when retrieving data. If not specified, will default to
        /// <see cref="RetryAttempts"/>. Will only reattempt on HTTP errors.
        /// </param>
        /// <typeparam name="T">The type of object to serialize to request JSON.</typeparam>
        /// <typeparam name="U">The type of object to deserialize from response JSON.</typeparam>
        /// <returns>The deserialized object returned from the endpoint.</returns>
        public async Task<U> PostJsonObjectAsync<T, U>(Uri uri, T value, int? attempts = null)
        {
            // Try to make the request multiple times if HTTP errors occur.
            if (!attempts.HasValue) attempts = RetryAttempts;
            try
            {
                HttpContent content = JsonContent.Create<T>(value, options: JsonOptions);
                HttpResponseMessage response = await Client.PostAsync(uri, content);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<U>(options: JsonOptions);
            }
            catch (HttpRequestException)
            {
                if (--attempts <= 0) throw;
                else return await PostJsonObjectAsync<T, U>(uri, value, attempts);
            }
        }
        /// <summary>
        /// Posts an object to a specified URI by first converting to a JSON object. Throws an HTTP error if one has
        /// occurred. Will attempt to run multiple times in case of HTTP error.
        /// </summary>
        /// <param name="uri">The URI to post data to.</param>
        /// <param name="value">The value of the data.</param>
        /// <param name="attempts">
        /// The number of attempts to use when retrieving data. If not specified, will default to
        /// <see cref="RetryAttempts"/>. Will only reattempt on HTTP errors.
        /// </param>
        /// <typeparam name="T">The type of object to serialize to request JSON.</typeparam>
        /// <returns>Nothing.</returns>
        public async Task PostJsonObjectAsync<T>(Uri uri, T value, int? attempts = null)
        {
            // Try to make the request multiple times if HTTP errors occur.
            if (!attempts.HasValue) attempts = RetryAttempts;
            try
            {
                HttpContent content = JsonContent.Create<T>(value, options: JsonOptions);
                HttpResponseMessage response = await Client.PostAsync(uri, content);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                if (--attempts <= 0) throw;
                else await PostJsonObjectAsync<T>(uri, value, attempts);
            }
        }

        /// <summary>
        /// Extracts the HyperThought access properties from a Base64 encoded key.
        /// </summary>
        /// <param name="key">The Base64 encoded key.</param>
        /// <returns>HyperThought access properties.</returns>
        private static HyperthoughtApiAccess ParseAccessKey(string key)
        {
            // Convert the Base64 encoded key to a JSON object.
            try
            {
                byte[] apiJson = Convert.FromBase64String(key);
                return JsonSerializer.Deserialize<HyperthoughtApiAccess>(apiJson);
            }
            catch (ArgumentNullException exception)
            {
                throw new HttpRequestException("No API key provided.", exception, HttpStatusCode.Unauthorized);
            }
            catch (FormatException exception)
            {
                throw new HttpRequestException("API key in invalid format.", exception, HttpStatusCode.Unauthorized);
            }
            catch (JsonException exception)
            {
                throw new HttpRequestException("API key in invalid format.", exception, HttpStatusCode.Unauthorized);
            }
        }
    }
}