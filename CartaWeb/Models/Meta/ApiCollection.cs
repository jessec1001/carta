using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CartaWeb.Models.Meta
{
    /// <summary>
    /// Represents a collection of API endpoints.
    /// </summary>
    public class ApiCollection
    {
        /// <summary>
        /// Gets or sets the name of the collection.
        /// </summary>
        /// <value>The human-readable name of the collection.</value>
        [JsonPropertyName("name")]
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the description of the collection.
        /// </summary>
        /// <value>The collection description.</value>
        [JsonPropertyName("description")]
        public string Description { get; set; }
        /// <summary>
        /// Gets or sets the set of endpoints.
        /// </summary>
        /// <value>The set of endpoints contained in the collection.</value>
        [JsonPropertyName("endpoints")]
        public List<ApiEndpoint> Endpoints { get; set; }
    }
}