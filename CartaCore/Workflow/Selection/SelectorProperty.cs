using System.Linq;

using CartaCore.Data;

namespace CartaCore.Workflow.Selection
{
    /// <summary>
    /// Represents a selection of vertices based on a regular expression match of vertex labels.
    /// </summary>
    public class SelectorProperty : SelectorBase
    {
        /// <inheritdoc />
        public override string Type => "property";

        /// <summary>
        /// The regular expression pattern to use to match labels on vertices.
        /// </summary>
        public string Property { get; set; }

        /// <inheritdoc />
        public override bool Contains(Vertex vertex)
        {
            if (vertex.Properties.Any(property => property.Identifier.ToString().ToLower() == Property.ToLower())) return true;
            return false;
        }
    }
}