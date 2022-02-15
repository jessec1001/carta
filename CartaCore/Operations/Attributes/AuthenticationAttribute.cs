using System;
using System.Collections.Generic;
using CartaCore.Typing.Conversion;

namespace CartaCore.Operations.Attributes
{
    // TODO: In the future, we need to make sure that the authentication information, however sent, is not accessible to
    //       any operations that are not explicitly marked to receive authentication.
    // TODO: Also consider making the authentication information not sent in the same request to start an operation but
    //       must instead be provided as a job task similar to a file upload.
    // TODO: This attribute should also block the authentication field from being sent in the operation schema.
    // A particular implementation might look like: OperationAuthenticationAttribute : IFieldTypeAttribute, IOperationTaskAttribute, ISchemaAttribute.

    /// <summary>
    /// An attribute that marks a particular operation field as requiring authentication.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class OperationAuthenticationAttribute : Attribute, ITypeConverterAttribute, IOperationContextAttribute
    {
        // TODO: Implement this type converter so that it can modify the results of the dictionary type converter.
        private class AuthenticationTypeConverter : TypeConverter { }

        /// <inheritdoc />
        public OperationContext Context { get; set; }
        /// <summary>
        /// The key to look up the authentication information in the context.
        /// </summary>
        public string Key { get; init; }

        /// <summary>
        /// Creates a new instance of the <see cref="OperationAuthenticationAttribute"/> class.
        /// </summary>
        /// <param name="key">The authentication key name.</param>
        public OperationAuthenticationAttribute(string key) => Key = key;

        /// <inheritdoc />
        public void ApplyConverter(IList<TypeConverter> converters)
        {
            // TODO: We need to modify the dictionary type converter to allow for partial converters.
        }
    }
}