using System.Linq;

using CartaCore.Data;
using CartaCore.Serialization.Json;

namespace CartaCore.Workflow.Selection
{
    /// <summary>
    /// Represents a selection of vertices based on a regular expression match of vertex labels.
    /// </summary>
    [DiscriminantDerived("property name")]
    public class SelectorPropertyName : SelectorRegexBase
    {
        /// <inheritdoc />
        public override bool Contains(IVertex vertex)
        {
            if (Regex is null) return true;
            return vertex.Properties.Any(property => Regex.IsMatch(property.Identifier.ToString()));
        }
    }
}