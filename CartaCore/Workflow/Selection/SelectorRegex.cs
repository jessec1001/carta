using System;
using System.Text.RegularExpressions;

using QuikGraph;

using CartaCore.Data;

namespace CartaCore.Workflow.Selection
{
    using FreeformGraph = IMutableVertexAndEdgeSet<FreeformVertex, FreeformEdge>;

    /// <summary>
    /// Represents a selection of vertices based on a regular expression match of vertex labels.
    /// </summary>
    public class SelectorRegex : SelectorBase
    {
        private Regex Regex;
        private string RegexPattern;

        /// <summary>
        /// The maximum amount of time that an instance of the <see cref="SelectorRegex"/> class will be able to filter
        /// before aborting its operation.
        /// </summary>
        public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(1.0);
        /// <summary>
        /// The regular expression pattern to use to match labels on vertices.
        /// </summary>
        public string Pattern
        {
            get => RegexPattern;
            set
            {
                RegexPattern = value;

                // We need to recreate the regular expression matcher when the pattern is changed.
                if (!(RegexPattern is null) && Regex is null)
                {
                    Regex = new Regex
                    (
                        Pattern,
                        RegexOptions.Multiline |
                        RegexOptions.Compiled,
                        Timeout
                    );
                }
            }
        }

        /// <inheritdoc />
        public override FreeformGraph Filter(FreeformGraph graph)
        {
            if (Regex is null) return graph;
            return FilterByCondition
            (
                graph,
                vertex => !(vertex.Label is null) && Regex.IsMatch(vertex.Label)
            );
        }
    }
}