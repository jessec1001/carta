using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CartaWeb.Models.Meta
{
    /// <summary>
    /// Represents an API endpoint.
    /// </summary>
    public class ApiEndpoint
    {
        /// <summary>
        /// The HTTP method of the endpoint.
        /// </summary>
        /// <value>The HTTP method.</value>
        [JsonPropertyName("method")]
        public ApiMethod Method { get; set; }
        /// <summary>
        /// The route path of the endpoint.
        /// </summary>
        /// <value>The path.</value>
        [JsonPropertyName("path")]
        public string Path { get; set; }
        /// <summary>
        /// The list of parameters of the endpoint.
        /// </summary>
        /// <value>The parameters.</value>
        [JsonPropertyName("parameters")]
        public List<ApiParameter> Parameters { get; set; }
    }
}