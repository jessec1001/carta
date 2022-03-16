using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

using CartaCore.Serialization.Json;

namespace CartaWeb.Models.Meta
{
    /// <summary>
    /// Represents the HTTP method of an API endpoint.
    /// </summary>
    [JsonConverter(typeof(JsonFullStringEnumConverter))]
    public enum ApiMethod
    {
        /// <summary>
        /// HTTP Get
        /// </summary>
        [EnumMember(Value = "get")]
        GET,
        /// <summary>
        /// HTTP Head
        /// </summary>
        [EnumMember(Value = "head")]
        HEAD,
        /// <summary>
        /// HTTP Patch
        /// </summary>
        [EnumMember(Value = "patch")]
        PATCH,
        /// <summary>
        /// HTTP Put
        /// </summary>
        [EnumMember(Value = "put")]
        PUT,
        /// <summary>
        /// HTTP Post
        /// </summary>
        [EnumMember(Value = "post")]
        POST,
        /// <summary>
        /// HTTP Delete
        /// </summary>
        [EnumMember(Value = "delete")]
        DELETE,
        /// <summary>
        /// HTTP Options
        /// </summary>
        [EnumMember(Value = "options")]
        OPTIONS
    }

    /// <summary>
    /// Provides methods to convert to and from HTTP methods from their corresponding attributes.
    /// </summary>
    public static class RouteExtensions
    {
        /// <summary>
        /// Get the API method type from and HTTP method attribute.
        /// </summary>
        /// <param name="attr">The HTTP method attribute.</param>
        /// <returns>The API method type.</returns>
        public static ApiMethod GetMethodType(this HttpMethodAttribute attr)
        {
            return attr switch
            {
                HttpHeadAttribute _ => ApiMethod.HEAD,
                HttpPatchAttribute _ => ApiMethod.PATCH,
                HttpPutAttribute _ => ApiMethod.PUT,
                HttpPostAttribute _ => ApiMethod.POST,
                HttpDeleteAttribute _ => ApiMethod.DELETE,
                HttpOptionsAttribute _ => ApiMethod.OPTIONS,
                _ => ApiMethod.GET,
            };
        }
    }
}