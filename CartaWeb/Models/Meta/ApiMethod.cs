using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

using CartaCore.Serialization.Json;

namespace CartaWeb.Models.Meta
{
    [JsonConverter(typeof(JsonExactStringEnumConverter))]
    public enum ApiMethod
    {
        [EnumMember(Value = "get")]
        GET,
        [EnumMember(Value = "head")]
        HEAD,
        [EnumMember(Value = "patch")]
        PATCH,
        [EnumMember(Value = "put")]
        PUT,
        [EnumMember(Value = "post")]
        POST,
        [EnumMember(Value = "delete")]
        DELETE,
        [EnumMember(Value = "options")]
        OPTIONS
    }

    public static class RouteExtensions
    {
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