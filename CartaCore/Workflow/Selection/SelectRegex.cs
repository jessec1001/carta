using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using CartaCore.Data;

namespace CartaCore.Workflow.Selection
{
    /// <summary>
    /// Represents a selection of vertices based on a regular expression match of vertex labels.
    /// </summary>
    public class SelectRegex
    {
        private Regex Regex;
        private string RegexPattern;

        /// <summary>
        /// The maximum amount of time that an instance of the <see cref="SelectRegex"/> class will be able to filter
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

        /// <summary>
        /// Filters a specified set of vertices based on whether the vertex label matches the specified regular
        /// expression pattern.
        /// </summary>
        /// <param name="graph">The graph containing the vertices.</param>
        /// <param name="ids">An enumerable of identifiers of the vertices to filter.</param>
        /// <returns>The filtered vertices.</returns>
        public IEnumerable<Guid> Filter(ISampledGraph graph, IEnumerable<Guid> ids)
        {
            // Filter out the vertices that have labels matching the regular expression. 
            foreach (Guid id in ids)
            {
                // Get the properties of the specified graph vertex.
                FreeformVertex vertex = graph.GetProperties(id);

                // Check if the vertex matches the regular expression.
                if ((Regex is null) || (!(vertex.Label is null) && Regex.IsMatch(vertex.Label)))
                    yield return id;
            }
        }
    }
}