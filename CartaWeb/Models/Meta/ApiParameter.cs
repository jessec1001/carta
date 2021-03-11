using System;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;

using CartaCore.Utility;

namespace CartaWeb.Models.Meta
{
    /// <summary>
    /// Represents a parameter that can be specified in an API endpoint.
    /// </summary>
    public class ApiParameter
    {
        /// <summary>
        /// The name of the parameter.
        /// </summary>
        /// <value>The parameter name.</value>
        [JsonPropertyName("name")]
        public string Name { get; set; }
        /// <summary>
        /// The type of the parameter.
        /// </summary>
        /// <value>The parameter type.</value>
        [JsonIgnore]
        public Type Type { get; set; }
        /// <summary>
        /// The type name of the parameter.
        /// </summary>
        /// <value>The parameter type name.</value>
        [JsonPropertyName("type")]
        public string TypeName
        {
            // We make type names more user-friendly by splitting code names.
            get => Regex.Replace(Type.TypeSerialize(), @"([a-z])([A-Z])", "$1 $2");
        }
        /// <summary>
        /// Gets or sets the format that the parameter is received in.
        /// </summary>
        /// <value>The parameter format.</value>
        [JsonPropertyName("format")]
        public ApiParameterFormat Format { get; set; }
        /// <summary>
        /// Gets or sets the parameter description.
        /// </summary>
        /// <value>A description string of text for the parameter.</value>
        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}