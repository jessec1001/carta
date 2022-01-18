using System;

namespace CartaCore.Operations.Attributes
{
    // TODO: In the future, we need to make sure that the authentication information, however sent, is not accessible to
    //       any operations that are not explicitly marked to receive authentication.
    // TODO: Also consider making the authentication information not sent in the same request to start an operation but
    //       must instead be provided as a job task similar to a file upload.
    // TODO: This attribute should also block the authentication field from being sent in the operation schema.

    // A particular implementation might look like: OperationAuthenticationAttribute : IFieldTypeAttribute, IOperationTaskAttribute, ISchemaAttribute
    [AttributeUsage(AttributeTargets.Property)]
    public class OperationAuthenticationAttribute : Attribute
    {
        public string Prefix { get; init; } = "authentication";
        public string Key { get; init; }

        public OperationAuthenticationAttribute(string key) => Key = key;
    }
}