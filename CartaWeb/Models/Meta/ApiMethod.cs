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
    [JsonConverter(typeof(JsonExactStringEnumConverter))]
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
            switch (attr)
            {
                case HttpHeadAttribute _:
                    return ApiMethod.HEAD;
                case HttpPatchAttribute _:
                    return ApiMethod.PATCH;
                case HttpPutAttribute _:
                    return ApiMethod.PUT;
                case HttpPostAttribute _:
                    return ApiMethod.POST;
                case HttpDeleteAttribute _:
                    return ApiMethod.DELETE;
                case HttpOptionsAttribute _:
                    return ApiMethod.OPTIONS;
                case HttpGetAttribute _:
                default:
                    return ApiMethod.GET;
            }
        }
    }
}