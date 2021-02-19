using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using CartaCore.Workflow.Selection;

namespace CartaWeb.Models.Selections
{
    public class SelectorBaseConverter : JsonConverter<SelectorBase>
    {
        /// <inheritdoc />
        public override SelectorBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException();

            using (JsonDocument document = JsonDocument.ParseValue(ref reader))
            {
                if (!document.RootElement.TryGetProperty("type", out JsonElement typeProperty))
                    throw new JsonException();

                Type baseType;
                switch (typeProperty.GetString())
                {
                    case "regex":
                        baseType = typeof(SelectorRegex);
                        break;
                    case "property":
                        baseType = typeof(SelectorProperty);
                        break;
                    default:
                        throw new JsonException();
                }

                string json = document.RootElement.GetRawText();
                return (SelectorBase)JsonSerializer.Deserialize(json, baseType, options);
            }
        }
        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, SelectorBase value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}