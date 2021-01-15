using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CartaCore.Serialization.Json
{
    public class JsonExactStringEnumConverter :
        JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            // We can only convert enumerations.
            return typeToConvert.IsEnum;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            JsonConverter converter = (JsonConverter)Activator.CreateInstance(
                typeof(JsonExactStringEnumConverterInner<>).MakeGenericType(
                    new Type[] { typeToConvert }
                ),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { },
                culture: null
            );

            return converter;
        }

        private class JsonExactStringEnumConverterInner<TEnum> :
            JsonConverter<TEnum> where TEnum : struct, Enum
        {
            private Dictionary<string, TEnum> Map;
            private Dictionary<TEnum, string> InverseMap;

            public JsonExactStringEnumConverterInner()
            {
                TEnum[] values = Enum
                    .GetValues(typeof(TEnum))
                    .Cast<TEnum>()
                    .ToArray();

                Map = new Dictionary<string, TEnum>();
                InverseMap = new Dictionary<TEnum, string>();

                InverseMap = values.ToDictionary(
                    value => value,
                    value => typeof(TEnum)
                        .GetField(Enum.GetName(typeof(TEnum), value))
                        .GetCustomAttributes<EnumMemberAttribute>(false)
                        .Select(attr => attr.Value)
                        .FirstOrDefault()
                );
                Map = InverseMap.Keys.ToDictionary(
                    key => InverseMap[key],
                    key => key
                );
            }

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

            public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(InverseMap[value]);
            }
        }
    }
}