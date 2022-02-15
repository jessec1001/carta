using NJsonSchema;

namespace CartaCore.Operations.Attributes
{
    /// <summary>
    /// A base interface for attributes that modify a JSON schema.
    /// </summary>
    public interface ISchemaModifierAttribute
    {
        /// <summary>
        /// Modifies the specified schema.
        /// </summary>
        /// <param name="schema">The input schema.</param>
        /// <returns>The output schema after modification. Should return null if the schema should be discard.</returns>
        JsonSchema ModifySchema(JsonSchema schema);
    }
}