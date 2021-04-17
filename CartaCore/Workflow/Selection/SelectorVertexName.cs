using System.Text.RegularExpressions;
using System.Threading.Tasks;

using CartaCore.Data;
using CartaCore.Serialization;

namespace CartaCore.Workflow.Selection
{
    /// <summary>
    /// Represents a selection of vertices based on a regular expression match of vertex labels.
    /// </summary>
    [DiscriminantDerived("vertexName")]
    public class SelectorVertexName : SelectorRegexBase
    {
        /// <inheritdoc />
        public override Task<bool> ContainsVertex(IVertex vertex)
        {
            if (Regex is null) return Task.FromResult(true);
            return Task.FromResult(!(vertex.Label is null) && Regex.IsMatch(vertex.Label));
        }
    }
}