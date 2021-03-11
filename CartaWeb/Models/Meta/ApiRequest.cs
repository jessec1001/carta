using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CartaWeb.Models.Meta
{
    /// <summary>
    /// Represents a sample request that can be made to an API endpoint.
    /// </summary>
    public class ApiRequest
    {
        /// <summary>
        /// Gets or sets the name of the request.
        /// </summary>
        /// <value>The name of the request.</value>
        [JsonPropertyName("name")]
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the body of the request.
        /// </summary>
        /// <value>The body of the request as a arbitrarily complex JSON object.</value>
        [JsonPropertyName("body")]
        public JsonDocument Body { get; set; }
        /// <summary>
        /// Gets or sets the arguments of the request.
        /// </summary>
        /// <value>The arguments of the request that will be send to the endpoint.</value>
        [JsonPropertyName("arguments")]
        public Dictionary<string, string> Arguments { get; set; }
    }
}