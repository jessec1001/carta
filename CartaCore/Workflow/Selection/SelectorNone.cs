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
        public override bool Contains(IVertex vertex)
        {
            return false;
        }
    }
}