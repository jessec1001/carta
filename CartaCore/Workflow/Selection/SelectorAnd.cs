using System.Collections.Generic;
using System.Linq;

using CartaCore.Data;
using CartaCore.Serialization.Json;

namespace CartaCore.Workflow.Selection
{
    /// <summary>
    /// Represents a selection of vertices based on a logical AND of other selections.
    /// </summary>
    [DiscriminantDerived("and")]
    public class SelectorAnd : SelectorBase
    {
        /// <summary>
        /// Gets or sets the list of selectors to AND together.
        /// </summary>
        /// <returns>The list of selectors that are combined with a logical AND operator.</returns>
        public List<SelectorBase> Selectors { get; set; } = new List<SelectorBase>();

        /// <inheritdoc />
        public override bool Contains(IVertex vertex)
        {
            if (Selectors.Count == 0) return true;
            return Selectors.All(selector => selector.Contains(vertex));
        }
    }
}