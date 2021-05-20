using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CartaCore.Serialization.Json
{
    /// <summary>
    /// Converts between JSON objects and types that have the <see cref="DiscriminantBaseAttribute"/> or
    /// <see cref="DiscriminantDerivedAttribute"/> attribute using a discriminant string value specified on the base
    /// class.
    /// </summary>
    public class JsonDiscriminantConverter : JsonConverterFactory
    {
        /// <inheritdoc />
        public override bool CanConvert(Type typeToConvert)
        {
            // We can convert types that that have the discriminant base attribute but are not derived and have the
            // discriminant derived attribute.
            if (typeToConvert.GetCustomAttribute<DiscriminantDerivedAttribute>() is not null) return false;
            if (typeToConvert.GetCustomAttribute<DiscriminantBaseAttribute>() is not null) return true;
            return false;
        }

        /// <inheritdoc />
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            // We need to create the appropriate converter with the specified type.
            JsonConverter converter = (JsonConverter)Activator.CreateInstance
            (
                typeof(JsonDiscriminantConverterInner<>).MakeGenericType(new Type[] { typeToConvert }),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { },
                culture: null
            );
            return converter;
        }

        private class JsonDiscriminantConverterInner<T> : JsonConverter<T>
        {
            /// <inheritdoc />
            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                // Get the discriminant name from the custom attribute.
                DiscriminantBaseAttribute baseAttr = typeToConvert.GetCustomAttribute<DiscriminantBaseAttribute>();
                string discriminantName = baseAttr.DiscriminantName;

                // When reading the base class, we should check for the discriminant and try to infer the derived class
                // from it. If this fails, just try to deserialize the base class.
                using (JsonDocument document = JsonDocument.ParseValue(ref reader))
                {
                    if (!document.RootElement.TryGetProperty(discriminantName, out JsonElement discriminantProperty))
                        throw new JsonException();

                    // Find the derived type from the discriminant.
                    string discriminant = discriminantProperty.GetString();
                    Discriminant.TryGetType(typeToConvert, discriminant, out DiscriminantType derivedType);

                    // Deserialize the derived type if possible.
                    if (derivedType is null)
                        throw new JsonException();
                    else
                        return (T)JsonSerializer.Deserialize(document.RootElement.GetRawText(), derivedType.Type, options);
                }
            }
            /// <inheritdoc />
            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                // Get the discriminant name and value.
                Type typeToConvert = value.GetType();
                DiscriminantBaseAttribute baseAttr = typeToConvert.GetCustomAttribute<DiscriminantBaseAttribute>();
                DiscriminantDerivedAttribute derivedAttr = typeToConvert.GetCustomAttribute<DiscriminantDerivedAttribute>();
                if (derivedAttr is null)
                    throw new JsonException();
                string discriminantName = baseAttr.DiscriminantName;
                string discriminant = derivedAttr.Discriminant;

                // We must get the default JSON document for the value.
                byte[] json = JsonSerializer.SerializeToUtf8Bytes(value, typeToConvert, options);
                using (JsonDocument document = JsonDocument.Parse(json))
                {
                    // Check that the object we are trying to write is actually in object format.
                    if (document.RootElement.ValueKind != JsonValueKind.Object)
                        throw new JsonException();

                    // We splice the discriminator into the default document.
                    writer.WriteStartObject();
                    writer.WritePropertyName(discriminantName);
                    writer.WriteStringValue(discriminant);
                    foreach (JsonProperty property in document.RootElement.EnumerateObject())
                        property.WriteTo(writer);
                    writer.WriteEndObject();
                    writer.Flush();
                }
            }
        }
    }
}