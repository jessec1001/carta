using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;
namespace CartaWeb.Models.DocumentItem
{
    public class JsonSecretConverter : JsonConverter<Item>
    {
        public bool Secret { get; private init; }

        public JsonSecretConverter(bool secret)
        {
            Secret = secret;
        }

        public override Item Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

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
