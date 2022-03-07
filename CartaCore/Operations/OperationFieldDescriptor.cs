using System;
using System.Collections.Generic;
using CartaCore.Documentation;
using NJsonSchema;

namespace CartaCore.Operations
{
    /// <summary>
    /// A description of an operation field including its name, type, and description.
    /// </summary>
    public class OperationFieldDescriptor
    {
        /// <summary>
        /// The name of the field.
        /// If not specified via attribute, this will be the name of the property.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The type of the field.
        /// </summary>
        public Type Type { get; set; }
        /// <summary>
        /// The set of custom attributes that have been applied to this field.
        /// These do not need to programmatically be applied to the field, but they can be constructed by other means.
        /// </summary>
        public IList<Attribute> Attributes { get; set; }
        /// <summary>
        /// The JSON schema that describes the field to a client.
        /// </summary>
        public JsonSchema Schema { get; set; }
    }
}