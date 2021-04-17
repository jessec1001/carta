using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CartaCore.Data;
using CartaCore.Serialization;

namespace CartaCore.Workflow.Selection
{
    /// <summary>
    /// Represents a selection of no vertices.
    /// </summary>
    [DiscriminantDerived("none")]
    public class SelectorNone : Selector
    {
        /// <inheritdoc />
        public override Task<bool> ContainsVertex(IVertex vertex)
        {
            return Task.FromResult(false);
        }
        public override Task<bool> ContainsProperty(Property property)
        {
            return Task.FromResult(false);
        }
        public override Task<bool> ContainsValue(object value)
        {
            return Task.FromResult(false);
        }

        public override IAsyncEnumerable<IVertex> GetVertices()
        {
            return Enumerable.Empty<IVertex>().ToAsyncEnumerable();
        }
    }
}