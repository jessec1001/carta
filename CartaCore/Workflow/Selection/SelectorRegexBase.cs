using System;
using System.Text.RegularExpressions;

namespace CartaCore.Workflow.Selection
{
    /// <summary>
    /// Represents a base selection based on a regular expression match.
    /// </summary>
    public abstract class SelectorRegexBase : SelectorBase
    {
        /// <summary>
        /// The regular expression pattern that can be used to test for matches in naming.
        /// </summary>
        protected Regex Regex;
        private string RegexPattern = string.Empty;

        /// <summary>
        /// The maximum amount of time that an instance of the <see cref="SelectorVertexName"/> class will be able to filter
        /// before aborting its operation.
        /// </summary>
        public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(1.0);
        /// <summary>
        /// The regular expression pattern to use to match naming.
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
    }
}