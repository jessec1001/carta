using System.Threading.Tasks;

using CartaCore.Data;
using CartaCore.Serialization;

namespace CartaCore.Workflow.Selection
{
    /// <summary>
    /// Represents a selection of vertices based on a regular expression match of vertex labels.
    /// </summary>
    [DiscriminantDerived("propertyName")]
    public class SelectorPropertyName : SelectorRegexBase
    {
        public override Task<bool> ContainsProperty(Property property)
        {
            if (Regex is null) return Task.FromResult(true);
            return Task.FromResult(Regex.IsMatch(property.Identifier.ToString()));
        }
    }
}