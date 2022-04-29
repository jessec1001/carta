using System;
using System.Collections.Generic;
using NJsonSchema;

namespace CartaCore.Operations.Attributes
{
    /// <summary>
    /// An attribute that marks the type of UI widget to use for a field.
    /// If this attribute is not added to an operation field, the default widget is used.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class FieldUiExtensionAttribute : Attribute, ISchemaModifierAttribute
    {
        /// <summary>
        /// The key of the extension.
        /// </summary>
        public string ExtensionField { get; }
        /// <summary>
        /// The value of the extension.
        /// </summary>
        public object ExtensionValue { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="FieldUiExtensionAttribute"/> class with the specified extension
        /// key and value.
        /// </summary>
        /// <param name="field">The extension field.</param>
        /// <param name="value">The extension value.</param>
        public FieldUiExtensionAttribute(string field, object value)
        {
            ExtensionField = field;
            ExtensionValue = value;
        }

        /// <inheritdoc />
        public JsonSchema ModifySchema(JsonSchema schema)
        {
            if (schema is null) return schema;
            if (schema.ExtensionData is null) schema.ExtensionData = new Dictionary<string, object>();
            schema.ExtensionData[$"ui:{ExtensionField}"] = ExtensionValue;
            return schema;
        }
    }
}