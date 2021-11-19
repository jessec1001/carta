using System;

namespace CartaCore.Utilities
{
    /// <summary>
    /// Extensions provided for working with strings with common applications.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Checks if a supstring (superstring) contains a (non-contiguous) subsequence (substring) of characters.
        /// </summary>
        /// <param name="supstring">The supstring.</param>
        /// <param name="substring">The substring.</param>
        /// <returns>
        /// <c>true</c> if there existed a subsequence of the supstring equal to the substring; otherwise <c>false</c>.
        /// </returns>
        public static bool ContainsSubsequence(this string supstring, string substring)
        {
            int supIndex = 0;
            int subIndex = 0;

            // Keep working through the superstring checking if we match the current substring character.
            // If we make it through the entire substring, the subsequence is matched.
            while (supIndex < supstring.Length)
            {
                if (subIndex >= substring.Length) break;
                if (substring[subIndex] == supstring[supIndex]) subIndex++;
                supIndex++;
            }
            return subIndex == substring.Length;
        }

        /// <summary>
        /// Computes the [Levenshtein distance](https://en.wikipedia.org/wiki/Levenshtein_distance) between two strings.
        /// This distance measures the number of atomic string actions that must be performed to transform a source
        /// string into a target string. These atomic actions include character insertion, deletion, and substitution.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="target">The target string.</param>
        /// <param name="costReplace">The cost of replacing a character.</param>
        /// <param name="costLengthen">The cost of changing the length of a string.</param>
        /// <returns>The Levenshtein distance between strings.</returns>
        public static double LevenshteinDistance(
            this string source,
            string target,
            double costReplace = 1.0,
            double costLengthen = 1.0)
        {
            // Initialize the full-size distance matrix for computing prefix distances.
            double[,] distanceMatrix = new double[source.Length + 1, target.Length + 1];

            // Set the initial conditions for the source and target strings.
            // This represents the action costs of adding all of the letters from the opposite string from the empty.
            for (int i = 0; i <= source.Length; i++)
                distanceMatrix[i, 0] = i * costLengthen;
            for (int j = 0; j <= target.Length; j++)
                distanceMatrix[0, j] = j * costLengthen;

            // We determine the distance between any two prefixes by using the least-cost previous cost of inserting,
            // deleting, or substituting.
            for (int j = 1; j <= target.Length; j++)
            {
                for (int i = 1; i <= source.Length; i++)
                {
                    // If the characters match, we don't need to substitute.
                    double addReplace = 0;
                    if (source[i - 1] != target[j - 1])
                        addReplace = costReplace;

                    // Determine the cost of string actions up to this point.
                    distanceMatrix[i, j] = Math.Min(
                        Math.Min(
                            distanceMatrix[i - 1, j] + costLengthen,    // Deletion
                            distanceMatrix[i, j - 1] + costLengthen     // Insertion
                        ),
                        distanceMatrix[i - 1, j - 1] + addReplace   // Substitution
                    );
                }
            }

            // The prefixes are equal to the original strings at the end of the matrix.
            // This distance cost is the distance of the original strings.
            return distanceMatrix[source.Length, target.Length];
        }

        /// <summary>
        /// Measures the similarity between two strings by measuring contiguous subsequences between them. A multiplier
        /// is used to make longer matching subsequences indicate more similarity.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="target">The target string.</param>
        /// <param name="continuityPower">
        /// The power of similarity for contiguous sequences. Should be greater than or equal to 1.
        /// </param>
        /// <param name="continuity">The current length of continuity.</param>
        /// <param name="depth">The depth at which to compute the similarity.</param>
        /// <returns>The similarity measure.</returns>
        public static double SubsequenceSimilarity(
            this string source,
            string target,
            double continuityPower = 2,
            int continuity = 0,
            int depth = 4
        )
        {
            // If either of the strings are empty, or depth reached, there is no similarity (vacuumously unsimilar).
            // Otherwise, we can procede with the recursive algorithm.
            if (depth == 0 || source.Length == 0 || target.Length == 0)
                return Math.Pow(continuity, continuityPower);

            // For each index of the target, we find the similarity of substrings and return the maximum.
            double maximum = 0;
            for (int k = 0; k < target.Length; k++)
            {
                // Get the similarity at this index of the target.
                double similarity;
                string sourceSubstring = source[1..];
                string targetSubstring = target[(k + 1)..];
                if (source[0] == target[k])
                {
                    // If the strings match at the beginning, the continuity increases (no addition).
                    // If the strings match elsewhere, the continuity restarts.
                    if (k == 0)
                    {
                        similarity = SubsequenceSimilarity(
                            sourceSubstring,
                            targetSubstring,
                            continuityPower,
                            continuity + 1,
                            depth - 1
                        );
                    }
                    else
                    {
                        similarity = SubsequenceSimilarity(
                            sourceSubstring,
                            targetSubstring,
                            continuityPower,
                            1,
                            depth - 1
                        ) + Math.Pow(continuity, continuityPower);
                    }
                }
                else
                {
                    // If the strings do not match, the continuity is none.
                    similarity = SubsequenceSimilarity(
                        sourceSubstring,
                        targetSubstring,
                        continuityPower,
                        0,
                        depth - 1
                    ) + Math.Pow(continuity, continuityPower);
                }

                // Check if this similarity is the maximum.
                maximum = Math.Max(maximum, similarity);
            }
            return maximum;
        }
    }
}