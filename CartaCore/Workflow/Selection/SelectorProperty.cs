using System;
using System.Linq;
using System.Text.RegularExpressions;

using CartaCore.Data.Freeform;

namespace CartaCore.Workflow.Selection
{
    /// <summary>
    /// Represents a selection of vertices based on a regular expression match of vertex labels.
    /// </summary>
    public class SelectorProperty : SelectorBase
    {
        public override string Type => "property";

        /// <summary>
        /// The regular expression pattern to use to match labels on vertices.
        /// </summary>
        public string Property { get; set; }

        /// <inheritdoc />
        public override bool Contains(FreeformVertex vertex)
        {
            foreach (FreeformProperty property in vertex.Properties)
                Console.WriteLine($"{property.Identifier.ToString().ToLower()} - {Property.ToLower()}");

            if (vertex.Properties.Any(property => property.Identifier.ToString().ToLower() == Property.ToLower())) return true;
            return false;
        }
    }
}