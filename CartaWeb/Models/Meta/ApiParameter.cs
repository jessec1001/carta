using System;
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
            get => Type.ToFriendlyString();
            set => Type = value.ToFriendlyType();
        }
    }
}