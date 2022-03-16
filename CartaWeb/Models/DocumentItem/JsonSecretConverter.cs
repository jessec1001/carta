using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;

namespace CartaWeb.Models.DocumentItem
{
    /// <summary>
    /// Attribute that can be used to denote that a property on a CartaWeb.Models.DocumentItem.Item object is
    /// secret.
    /// </summary>
    sealed class SecretAttribute : System.Attribute { }

    /// <summary>
    /// Converter that splits the serialization of CartaWeb.Models.DocumentItem.Item objects by properties with
    /// and without the Secret attribute.
    /// </summary>
    public class JsonSecretConverter : JsonConverter<Item>
    {
        /// <summary>
        /// Flag determining whether properties with the Secret attribute should be serialized, or
        /// whether properties without the Secret attribute should be serialized. 
        /// </summary>
        public bool Secret { get; private init; }

        /// <summary>
        /// Constructor
        /// <param name="secret">Secret flag</param>
        /// </summary>
        public JsonSecretConverter(bool secret)
        {
            Secret = secret;
        }

        /// <inheritdoc />
        public override Item Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, Item value, JsonSerializerOptions options)
        {
            Item copiedItem = value.Copy();
            bool hasProperty = false;
            foreach (PropertyInfo property in copiedItem.GetType().GetProperties())
            {
                if (Secret)
                {
                    if (property.GetCustomAttribute<SecretAttribute>() is null &&
                        property.GetCustomAttribute<JsonIgnoreAttribute>() is null)
                    {
                        property.SetValue(copiedItem, null);
                    }
                    else if (property.GetCustomAttribute<JsonIgnoreAttribute>() is null
                        && property.GetValue(copiedItem) is not null )
                    {
                        hasProperty = true;
                    }
                }
                else
                {
                    if (property.GetCustomAttribute<SecretAttribute>() is not null)
                    {
                        property.SetValue(copiedItem, null);
                    }
                    else if (property.GetCustomAttribute<JsonIgnoreAttribute>() is null
                        && property.GetValue(copiedItem) is not null)
                    {
                        hasProperty = true;
                    }
                }
            }
            if (hasProperty) JsonSerializer.Serialize(writer, (object)copiedItem, options);
        }
    }
}
