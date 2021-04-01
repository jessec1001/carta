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
        public override bool ContainsProperty(Property property)
        {
            if (Regex is null) return true;
            return Regex.IsMatch(property.Identifier.ToString());
        }
    }
}