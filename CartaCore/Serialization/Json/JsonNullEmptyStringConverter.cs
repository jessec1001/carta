using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CartaCore.Serialization.Json
{
    /// <summary>
    /// Converts between JSON strings and <see cref="Nullable"/> struct types.
    /// </summary>
    public class JsonNullEmptyStringConverter : JsonConverterFactory
    {
        /// <inheritdoc />
        public override bool CanConvert(Type typeToConvert)
        {
            return Nullable.GetUnderlyingType(typeToConvert) != null;
        }

        /// <inheritdoc />
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            JsonConverter converter = (JsonConverter)Activator.CreateInstance(
                typeof(NullableEmptyStringConverterInner<>).MakeGenericType(
                    new Type[] { Nullable.GetUnderlyingType(typeToConvert) }
                ),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { options },
                culture: null
            );

            return converter;
        }

        /// <summary>
        /// Converts between JSON strings and <see cref="Nullable{T}"/> struct types.
        /// </summary>
        /// <typeparam name="T">The type of nullable.</typeparam>
        private class NullableEmptyStringConverterInner<T> : JsonConverter<T?> where T : struct
        {
            private readonly JsonConverter<T> Converter;

            /// <inheritdoc />
            public NullableEmptyStringConverterInner(JsonSerializerOptions options)
            {
                Converter = (JsonConverter<T>)options.GetConverter(typeof(T));
            }

            /// <inheritdoc />
            public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Null)
                    return null;
                if (reader.TokenType == JsonTokenType.String)
                {
                    if (string.IsNullOrEmpty(reader.GetString()))
                        return null;
                }

                if (Converter == null)
                    return JsonSerializer.Deserialize<T>(ref reader, options);
                else
                    return Converter.Read(ref reader, typeof(T), options);
            }

            /// <inheritdoc />
            public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
            {
                JsonSerializer.Serialize<T>(writer, value.Value, options);
            }

        }
    }
}