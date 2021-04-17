using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CartaCore.Serialization.Json
{
    public class JsonObjectEnumerableConverter : JsonConverter<List<object>>
    {
        private JsonObjectConverter Converter = new JsonObjectConverter();

        public override List<object> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            List<object> values = new List<object>();
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                values.Add(Converter.Read(ref reader, typeof(object), options));

            if (reader.TokenType != JsonTokenType.EndArray)
                throw new JsonException();

            return values;
        }
        public override void Write(Utf8JsonWriter writer, List<object> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (object obj in value)
                Converter.Write(writer, obj, options);
            writer.WriteEndArray();
        }
    }
}