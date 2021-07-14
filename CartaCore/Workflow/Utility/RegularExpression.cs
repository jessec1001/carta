using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace CartaCore.Workflow.Utility
{
    /// <summary>
    /// Contains utilities for working with and transforming regular expressions.
    /// </summary>
    public static class RegularExpressionUtility
    {
        private static readonly char[] EscapeCharacters = new char[]
        {
            '\\',
            '[',
            ']',
            '.',
            '*',
            '{',
            '}',
            '(',
            ')',
            '?',
            '+',
            '|'
        };

        /// <summary>
        /// Converts a string to a regular expression. If the string is surrounded by forward slashes '/', the string is
        /// assumed to already be a regular expression and is trimmed. Otherwise, the string is escaped and turned into
        /// an inclusion pattern.
        /// </summary>
        /// <param name="pattern">The string pattern to escape into a regular expression.</param>
        /// <returns>The escaped pattern created from the string pattern.</returns>
        public static string EscapeRegex(this string pattern)
        {
            // Patterns surrounded by forward slashes are assumed to be regular expression formatted already.
            // We trim the slashes and convert it directly to a regular expression.
            if (pattern.Length >= 2 && pattern[0] == '/' && pattern[^1] == '/')
            {
                return pattern.Substring(1, pattern.Length - 2);
            }
            // Patterns not surrounded by forward slashes are assumed to need escaping of regular expression characters.
            // The returned regular expression is then an inclusion expression.
            else
            {
                string escapedPattern = string.Empty;
                for (int k = 0; k < pattern.Length; k++)
                {
                    if (EscapeCharacters.Contains(pattern[k]))
                        escapedPattern += '\\' + pattern[k];
                    else
                        escapedPattern += pattern[k];
                }
                return escapedPattern;
            }
        }
    }
}
