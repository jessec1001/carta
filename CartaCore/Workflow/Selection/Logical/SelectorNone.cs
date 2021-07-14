using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using CartaCore.Data;
using CartaCore.Serialization;

using NJsonSchema.Annotations;

namespace CartaCore.Workflow.Selection
{
    /// <summary>
    /// Selects none of the vertices, properties, and values in a graph.
    /// </summary>
    [JsonSchemaFlatten]
    [DataContract]
    [DiscriminantDerived("none")]
    [DiscriminantSemantics(Name = "Select None", Group = "Logical")]
    public class SelectorNone : Selector
    {
        /// <inheritdoc />
        public override Task<bool> ContainsVertex(IVertex vertex)
        {
            return Task.FromResult(false);
        }
        /// <inheritdoc />
        public override Task<bool> ContainsProperty(Property property)
        {
            return Task.FromResult(false);
        }
        /// <inheritdoc />
        public override Task<bool> ContainsValue(object value)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public override IAsyncEnumerable<IVertex> GetVertices()
        {
            return Enumerable.Empty<IVertex>().ToAsyncEnumerable();
        }
    }
}