using System.Runtime.Serialization;

using NJsonSchema.Annotations;

using CartaCore.Serialization;

namespace CartaCore.Workflow.Selection
{
    /// <summary>
    /// Selects all of the vertices, properties, and values in a graph.
    /// </summary>
    [JsonSchemaFlatten]
    [DataContract]
    [DiscriminantDerived("all")]
    [DiscriminantSemantics(Name = "Select All", Group = "Logical")]
    public class SelectorAll : Selector { }
}