using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using CartaCore.Graphs;

namespace CartaCore.Serialization.Json
{
    /// <summary>
    /// Converts instances of <see cref="IProperty"/> to a concrete type.
    /// </summary>
    public class JsonPropertyConverter : JsonConverter<IProperty>
    {
        /// <inheritdoc />
        public override IProperty Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<Property>(ref reader, options);
        }
        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, IProperty value, JsonSerializerOptions options)
        {
            if (value is Property property)
                JsonSerializer.Serialize(writer, property, options);
        }
    }
}