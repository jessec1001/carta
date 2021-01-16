using System.Text.Json.Serialization;

namespace CartaWeb.Models.Meta
{
    public class ApiEndpoint
    {
        [JsonPropertyName("method")]
        public ApiMethod Method { get; set; }
        [JsonPropertyName("path")]
        public string Path { get; set; }
    }
}