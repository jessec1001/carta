using System.Text.RegularExpressions;

using CartaCore.Data;
using CartaCore.Serialization.Json;

namespace CartaCore.Workflow.Selection
{
    /// <summary>
    /// Represents a selection of vertices based on a regular expression match of vertex labels.
    /// </summary>
    [DiscriminantDerived("vertex name")]
    public class SelectorVertexName : SelectorRegexBase
    {
        /// <inheritdoc />
        public override bool ContainsVertex(IVertex vertex)
        {
            if (Regex is null) return true;
            return !(vertex.Label is null) && Regex.IsMatch(vertex.Label);
        }
    }
}