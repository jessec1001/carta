using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CartaCore.Serialization.Json
{
    public class JsonObjectConverter : JsonConverter<object>

    {
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
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
                default:
                    return null;
            }
        }
        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case bool boolValue:
                    writer.WriteBooleanValue(boolValue);
                    break;
                case double doubleValue:
                    writer.WriteNumberValue(doubleValue);
                    break;
                case int intValue:
                    writer.WriteNumberValue(intValue);
                    break;
                case string stringValue:
                    writer.WriteStringValue(stringValue);
                    break;
                case null:
                default:
                    writer.WriteNullValue();
                    break;
            }
        }
    }

}