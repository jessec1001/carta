using System;
using NJsonSchema;

namespace CartaCore.Operations.Attributes
{
    /// <summary>
    /// An attribute that specifies a default value for a field.
    /// If this attribute is not added to a field, the default value is the default for the type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class FieldDefaultAttribute : Attribute, ISchemaModifierAttribute
    {
        /// <summary>
        /// The default value to use for this field.
        /// </summary>
        public object Default { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="FieldDefaultAttribute"/> class with the specified default value.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        public FieldDefaultAttribute(object defaultValue)
        {
            Default = defaultValue;
        }

        /// <inheritdoc />
        public JsonSchema ModifySchema(JsonSchema schema)
        {
            if (schema is not null) schema.Default = Default;
            return schema;
        }
    }
}