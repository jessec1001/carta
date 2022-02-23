using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CartaCore.Serialization.Json
{
    /// <summary>
    /// Converts between JSON tokens and <see cref="object"/> types using basic inference on the token types.
    /// Only booleans, strings and numbers are permitted.
    /// </summary>
    public class JsonPrimativeConverter : JsonConverter<object>

    {
        /// <inheritdoc />
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // We serialize based on all possible types of token.
            // If we encounter an unexpected token type, we throw an exception.
            return reader.TokenType switch
            {
                JsonTokenType.False => false,
                JsonTokenType.True => true,
                JsonTokenType.Number => reader.GetDouble(),
                JsonTokenType.String => reader.GetString(),
                JsonTokenType.Null => null,
                _ => throw new JsonException("Unsupported JSON format. Only primitives (bool, string, number) allowed."),
            };
        }
        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            // Simply use the default serialization process for writing object values.
            JsonSerializer.Serialize(writer, value, options);
        }
    }

}