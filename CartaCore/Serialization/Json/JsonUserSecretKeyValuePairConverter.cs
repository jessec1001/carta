using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using CartaCore.Persistence;

namespace CartaCore.Serialization.Json
{

    /// <summary>
    /// Converts UserSecretKeyValuePairConverter objects with secret values masked by default.
    /// </summary>
    public class JsonUserSecretKeyValuePairConverter : JsonConverter<UserSecretKeyValuePair>
    {
        private readonly JsonObjectConverter _converter = new();
        private bool _maskSecret;

        /// <summary>
        /// Constructor.
        /// </summary>
        public JsonUserSecretKeyValuePairConverter (bool maskSecret = true)
        {
            _maskSecret = maskSecret;
        }

        /// <inheritdoc />
        public override UserSecretKeyValuePair Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            UserSecretKeyValuePair keyValuePair = new UserSecretKeyValuePair();
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                {
                    // Read the property name.
                    if (reader.TokenType != JsonTokenType.PropertyName)
                        throw new JsonException("Property name not found");
                    string propertyName = reader.GetString();

                    // Read the property value.
                    reader.Read();
                    object propertyValue = _converter.Read(ref reader, typeof(object), options);

                    if (propertyName == "key")
                    {
                        keyValuePair.Key = propertyValue.ToString();   
                    }
                    if (propertyName == "value")
                    {
                        if (_maskSecret)
                            keyValuePair.Value = new string('*', propertyValue.ToString().Length);
                        else
                            keyValuePair.Value = propertyValue;
                    }
                }
                if (reader.TokenType != JsonTokenType.EndObject) throw new JsonException("End object tag not found");
                return keyValuePair;
            }
            else
            {
                throw new JsonException("Start object tag not found");
            }
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, UserSecretKeyValuePair value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("key", value.Key);
            if (_maskSecret)
                writer.WriteString("value", new string('*', value.Value.ToString().Length));
            else
            {
                writer.WritePropertyName("value");
                _converter.Write(writer, value.Value, options);
            }
            writer.WriteEndObject();
        }
    }

}