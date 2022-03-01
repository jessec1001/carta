using System;
using NJsonSchema;

namespace CartaCore.Operations.Attributes
{
    /// <summary>
    /// An attribute that marks a field as being a dynamic field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class FieldDynamicAttribute : Attribute, IOperationJobAttribute, ISchemaModifierAttribute
    {
        public OperationJob Job { set => throw new NotImplementedException(); }

        public JsonSchema ModifySchema(JsonSchema schema)
        {
            // TODO: Figure out how to hide the schema that this attribute is attached to and expose schemas of the
            //       dynamic fields. Determine whether this should be built-in to the operation via get input/output
            //       fields/types (probably should partially due to the naming and connotation).
            throw new NotImplementedException();
        }
    }
}