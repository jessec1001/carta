using System;
using NJsonSchema;

namespace CartaCore.Operations.Attributes
{
    /// <summary>
    /// An attribute that marks a field as required.
    /// If this attribute is not added to a field, the field is not required.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class FieldRequiredAttribute : Attribute, ISchemaModifierAttribute
    {
        /// <inheritdoc />
        public JsonSchema ModifySchema(JsonSchema schema)
        {
            if (schema is not null) schema.IsNullableRaw = false;
            return schema;
        }
    }
}