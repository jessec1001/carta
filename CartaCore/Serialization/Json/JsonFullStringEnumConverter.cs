using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CartaCore.Serialization.Json
{
    /// <summary>
    /// Converts between JSON strings and <see cref="Enum"/> types using a case invariant match of any
    /// <see cref="EnumMemberAttribute"/> names applied to the <see cref="Enum"/> values.
    /// </summary>
    public class JsonFullStringEnumConverter : JsonConverterFactory
    {
        /// <inheritdoc />
        public override bool CanConvert(Type typeToConvert)
        {
            // We can only convert enumerations.
            return typeToConvert.IsEnum;
        }

        /// <inheritdoc />
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            JsonConverter converter = (JsonConverter)Activator.CreateInstance(
                typeof(JsonFullStringEnumConverterInner<>).MakeGenericType(
                    new Type[] { typeToConvert }
                ),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { },
                culture: null
            );

            return converter;
        }

        /// <summary>
        /// Converts between JSON strings and <typeparamref name="TEnum"/> types using a case invariant match of any
        /// <see cref="EnumMemberAttribute"/> names applied to the <typeparamref name="TEnum"/> values.
        /// </summary>
        /// <typeparam name="TEnum">The type of enumeration.</typeparam>
        private class JsonFullStringEnumConverterInner<TEnum> :
            JsonConverter<TEnum> where TEnum : struct, Enum
        {
            private Dictionary<string, TEnum> Map;
            private Dictionary<TEnum, string> InverseMap;

            /// <inheritdoc />
            public JsonFullStringEnumConverterInner()
            {
                TEnum[] values = Enum
                    .GetValues(typeof(TEnum))
                    .Cast<TEnum>()
                    .ToArray();

                Map = new Dictionary<string, TEnum>();
                InverseMap = new Dictionary<TEnum, string>();

                Map = values.ToDictionary(
                    value => typeof(TEnum)
                        .GetField(Enum.GetName(typeof(TEnum), value))
                        .GetCustomAttributes<EnumMemberAttribute>(false)
                        .Select(attr => attr.Value)
                        .FirstOrDefault(),
                    StringComparer.OrdinalIgnoreCase
                );
                InverseMap = Map.Keys.ToDictionary(
                    key => Map[key]
                );
            }

            /// <inheritdoc />
            public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.String)
                    throw new JsonException();

                bool success = Map.TryGetValue(reader.GetString(), out TEnum value);
                if (success)
                    return value;
                else
                    throw new JsonException();
            }

            /// <inheritdoc />
            public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(InverseMap[value]);
            }
        }
    }
}