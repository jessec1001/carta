using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CartaCore.Serialization.Json
{
    /// <summary>
    /// Converts between JSON tokens and <see cref="object"/> types using basic inference on the token types.
    /// </summary>
    public class JsonObjectConverter : JsonConverter<object>

    {
        /// <inheritdoc />
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // We serialize based on all possible types of token.
            // If we encounter an unexpected token type, we throw an exception.
            switch (reader.TokenType)
            {
                case JsonTokenType.False:
                    return false;
                case JsonTokenType.True:
                    return true;
                case JsonTokenType.Number:
                    return reader.GetDouble();
                case JsonTokenType.String:
                    return reader.GetString();
                case JsonTokenType.Null:
                    return null;
                default:
                    throw new JsonException();
            }
        }
        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            // Simply use the default serialization process for writing object values.
            JsonSerializer.Serialize(writer, value, options);
        }
    }

}