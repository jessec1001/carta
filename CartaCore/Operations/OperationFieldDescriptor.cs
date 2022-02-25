using System;
using System.Collections;
using System.Collections.Generic;
using NJsonSchema;

namespace CartaCore.Operations
{
    public class OperationFieldDescriptor
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public IList<Attribute> Attributes { get; set; }
        public JsonSchema Schema { get; set; }
    }
}