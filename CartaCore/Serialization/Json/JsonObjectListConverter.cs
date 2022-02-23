using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CartaCore.Serialization.Json
{
    /// <summary>
    /// Converts between a JSON array of tokens and a list of <see cref="object"/> types.
    /// </summary>
    public class JsonObjectListConverter : JsonConverter<List<object>>
    {
        // This converter is used to convert each value of the list,
        private readonly JsonObjectConverter Converter = new();

        /// <inheritdoc />
        public override List<object> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Make sure that our JSON has an array structure.
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            // Read in each value using a value-based converter.
            List<object> values = new();
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                values.Add(Converter.Read(ref reader, typeof(object), options));

            // Make sure that our JSON has an array structure.
            if (reader.TokenType != JsonTokenType.EndArray)
                throw new JsonException();

            return values;
        }
        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, List<object> value, JsonSerializerOptions options)
        {
            // Use default serialization to write out the objects.
            writer.WriteStartArray();
            foreach (object obj in value)
                JsonSerializer.Serialize(writer, obj, options);
            writer.WriteEndArray();
        }
    }
}