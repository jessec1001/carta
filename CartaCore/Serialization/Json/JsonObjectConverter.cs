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
        private static bool IsBoolean(JsonTokenType tokenType)
        {
            return tokenType == JsonTokenType.True || tokenType == JsonTokenType.False;
        }

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
                    bool sameTokenType = true;
                    JsonTokenType prevTokenType = JsonTokenType.Null;
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                    {
                        JsonTokenType tokenType = reader.TokenType;
                        array.Add(Read(ref reader, typeToConvert, options));
                        if (tokenType != prevTokenType &&
                            prevTokenType != JsonTokenType.Null &&
                            !IsBoolean(tokenType) && !IsBoolean(prevTokenType)) sameTokenType = false;
                        prevTokenType = tokenType;
                    }
                    if (reader.TokenType != JsonTokenType.EndArray) throw new JsonException();
                    if (sameTokenType)
                    {
                        switch (prevTokenType)
                        {
                            case JsonTokenType.String:
                                string[] stringArray = new string[array.Count];
                                for (int k = 0; k < array.Count; k++) stringArray[k] = (string)array[k];
                                return stringArray;
                            case JsonTokenType.Number:
                                double[] numberArray = new double[array.Count];
                                for (int k = 0; k < array.Count; k++) numberArray[k] = (double)array[k];
                                return numberArray;
                            case JsonTokenType.True:
                            case JsonTokenType.False:
                                bool[] boolArray = new bool[array.Count];
                                for (int k = 0; k < array.Count; k++) boolArray[k] = (bool)array[k];
                                return boolArray;
                        }
                    }
                    return array;

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