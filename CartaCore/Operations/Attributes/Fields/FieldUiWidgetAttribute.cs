using System;
using NJsonSchema;

namespace CartaCore.Operations.Attributes
{
    /// <summary>
    /// An attribute that marks the type of UI widget to use for a field.
    /// If this attribute is not added to an operation field, the default widget is used.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class FieldUiWidgetAttribute : Attribute, ISchemaModifierAttribute
    {
        /// <summary>
        /// The type of UI widget to use for this field.
        /// </summary>
        public string WidgetType { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="FieldUiWidgetAttribute"/> class with the specified widget type.
        /// </summary>
        /// <param name="widgetType">The type of UI widget. Used by the front-end for form generation.</param>
        public FieldUiWidgetAttribute(string widgetType)
        {
            WidgetType = widgetType;
        }

        /// <inheritdoc />
        public JsonSchema ModifySchema(JsonSchema schema)
        {
            if (schema is not null) schema.ExtensionData["ui:widget"] = WidgetType;
            return schema;
        }
    }
}