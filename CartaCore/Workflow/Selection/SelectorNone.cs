using CartaCore.Data;
using CartaCore.Serialization.Json;

namespace CartaCore.Workflow.Selection
{
    /// <summary>
    /// Represents a selection of no vertices.
    /// </summary>
    [DiscriminantDerived("none")]
    public class SelectorNone : SelectorBase
    {
        /// <inheritdoc />
        public override bool ContainsVertex(IVertex vertex)
        {
            return false;
        }
        public override bool ContainsProperty(Property property)
        {
            return false;
        }
        public override bool ContainsValue(object value)
        {
            return false;
        }
    }
}