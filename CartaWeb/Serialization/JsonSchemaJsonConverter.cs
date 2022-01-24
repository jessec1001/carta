using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using NJsonSchema;

namespace CartaWeb.Serialization
{
    public class JsonSchemaJsonConverter : JsonConverter<JsonSchema>
    {
        public override JsonSchema Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, JsonSchema value, JsonSerializerOptions options)
        {
            using JsonDocument document = JsonDocument.Parse(value.ToJson());
            document.RootElement.WriteTo(writer);
        }
    }
}