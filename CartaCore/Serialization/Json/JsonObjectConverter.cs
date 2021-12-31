using System;
using System.Collections.Generic;
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
                case JsonTokenType.StartArray:
                    // Try to convert into array.
                    List<object> array = new();
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                        array.Add(Read(ref reader, typeToConvert, options));
                    if (reader.TokenType != JsonTokenType.EndArray) throw new JsonException();
                    return array.ToArray();
                case JsonTokenType.StartObject:
                    // Try to convert into dictionary.
                    Dictionary<string, object> map = new();
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                    {
                        // Read the property name.
                        if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException();
                        string propertyName = reader.GetString();

                        // Read the property value.
                        reader.Read();
                        object propertyValue = Read(ref reader, typeToConvert, options);

                        map.Add(propertyName, propertyValue);
                    }
                    if (reader.TokenType != JsonTokenType.EndObject) throw new JsonException();
                    return map;
                default:
                    throw new JsonException("Unsupported JSON format.");
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