using System;
using NJsonSchema;

namespace CartaCore.Operations.Attributes
{
    /// <summary>
    /// An attribute that marks the display name of a field.
    /// If this attribute is not added to an operation field, the code-specified name is used.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class FieldNameAttribute : Attribute, ISchemaModifierAttribute
    {
        /// <summary>
        /// The display name to use for this field.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="FieldNameAttribute"/> class with the specified name.
        /// </summary>
        /// <param name="name">The display name of the field.</param>
        public FieldNameAttribute(string name)
        {
            Name = name;
        }

        /// <inheritdoc />
        public JsonSchema ModifySchema(JsonSchema schema)
        {
            if (schema is not null) schema.Title = Name;
            return schema;
        }
    }
}