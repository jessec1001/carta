using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CartaCore.Extensions.String
{
    /// <summary>
    /// Extensions provided for working with strings with common applications.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Replaces any consecutive whitespace characters with a single space and trims the string.
        /// </summary>
        /// <param name="str">The string to format.</param>
        /// <returns>The formatted string with whitespace contracted.</returns>
        public static string ContractWhitespace(this string str)
        {
            return Regex.Replace(str, @"\s+", " ").Trim();
        }

        /// <summary>
        /// Converts a string into a regular expression pattern. If the string is surrounded by forward slashes, it is
        /// assumed to be a regular expression pattern. Otherwise, it is assumed to be a literal substring and will be
        /// escaped.
        /// </summary>
        /// <param name="str">The string to convert.</param>
        /// <returns>A regular expression pattern.</returns>
        public static Regex ToRegexPattern(this string str)
        {
            if (str.StartsWith("/") && str.EndsWith("/"))
                return new Regex(str[1..^1]);
            else
                return new Regex(Regex.Escape(str));
        }

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
        /// Calculates the n-grams of a string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="n">The size of each n-gram.</param>
        /// <returns>An array of n-grams.</returns>
        public static string[] NGrams(this string str, int n)
        {
            // Check the n-gram size.
            if (n < 1) throw new ArgumentException("Size of n-grams must be at least 1");
            if (n > str.Length) return Array.Empty<string>();

            string[] ngrams = new string[str.Length - n + 1];
            for (int i = 0; i < ngrams.Length; i++)
                ngrams[i] = str.Substring(i, n);
            return ngrams;
        }

        /// <summary>
        /// Computes the [Hamming distnace](https://en.wikipedia.org/wiki/Hamming_distance) between two strings.
        /// Captures the number of character replacements between two strings where the order of characters is retained.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="target">The target string.</param>
        /// <returns>The Hamming distance between strings.</returns>
        public static int HammingDistance(this string source, string target)
        {
            // We modify the traditional hamming distance to extend to strings of different lengths.
            int length = Math.Min(source.Length, target.Length);
            int distance = 0;
            for (int i = 0; i < length; i++)
                if (source[i] != target[i]) distance++;
            distance += Math.Abs(source.Length - target.Length);
            return distance;
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
        /// Computes the bag intersection of the n-grams of two strings. The bag intersection is the number of n-grams
        /// that are common to both strings, one per occurrence in both strings.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="target">The target string.</param>
        /// <param name="sourceCount">The number of n-grams in the source string.</param>
        /// <param name="targetCount">The number of n-grams in the target string.</param>
        /// <param name="n">The size of n-grams.</param>
        /// <returns>The number of overlapping n-grams in both strings.</returns>
        public static int NGramOverlap(
            string source,
            string target,
            out int sourceCount,
            out int targetCount,
            int n = 1
        )
        {
            // Calculate the n-grams of the source and target strings.
            List<string> sourceNGrams = new(source.NGrams(n));
            List<string> targetNGrams = new(target.NGrams(n));
            sourceCount = sourceNGrams.Count;
            targetCount = targetNGrams.Count;

            // Count the number of n-grams that are shared between the source and target strings.
            // Notice that we avoid counting duplicates so that:
            // - "banana" and "na" have 1 shared bigram (rather than 2).
            // - "banana" and "nan" have 2 shared bigrams (rather than 4).
            // - "banana" and "nana" have 3 shared bigrams (rather than 9).
            int sharedNGrams = 0;
            foreach (string ngram in sourceNGrams)
            {
                if (targetNGrams.Remove(ngram))
                    sharedNGrams++;
            }

            // Return the Sorensen-Dice similarity.
            return sharedNGrams;
        }
        /// <summary>
        /// Computes the [Overlap Similarity](https://en.wikipedia.org/wiki/Overlap_coefficient) between two strings.
        /// Measures the overlap of bags of n-grams between the strings relative to the size of the smaller string.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="target">The target string.</param>
        /// <param name="n">The size of n-grams.</param>
        /// <returns>The Overlap Similarity.</returns>
        public static double OverlapSimilarity(this string source, string target, int n = 1)
        {
            // We compute |X|, |Y|, and |X and Y|.
            int sharedNGrams = NGramOverlap(source, target, out int sourceNGrams, out int targetNGrams, n);

            // Return the Overlap similarity: |X and Y| / min(|X|, |Y|).
            return (double)sharedNGrams / Math.Min(sourceNGrams, targetNGrams);
        }
        /// <summary>
        /// Computes the [Sorensen-Dice Similarity](https://en.wikipedia.org/wiki/S%C3%B8rensen%E2%80%93Dice_coefficient)
        /// between two strings. Measures the overlap of the bags of n-grams between the strings where the size of
        /// n-grams is configurable.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="target">The target string.</param>
        /// <param name="n">The size of n-grams.</param>
        /// <returns>The Sorensen-Dice similarity.</returns>
        public static double SorensenDiceSimilarity(this string source, string target, int n = 1)
        {
            // We compute |X and Y|, |X|, |Y|, and |X| + |Y|.
            int sharedNGrams = NGramOverlap(source, target, out int sourceNGrams, out int targetNGrams, n);
            int totalNGrams = sourceNGrams + targetNGrams;

            // Return the Sorensen-Dice similarity:  2|X and Y| / (|X| + |Y|).
            return 2.0 * sharedNGrams / totalNGrams;
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

        /// <summary>
        /// Calculates the [Term Frequency - Inverse Document Frequency](https://en.wikipedia.org/wiki/Tf%E2%80%93idf)
        /// measure for a given string among a corpus of documents.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="corpus">The corpus.</param>
        /// <returns>The TF-IDF for the string for each document.</returns>
        public static double[] TermFrequencyInverseDocumentFrequency(this string str, string[] corpus)
        {
            // We store the term frequency and document frequency for the word in the corpus.
            double[] tf = new double[corpus.Length];
            double df = 1;

            // Calculate frequencies for each document.
            for (int k = 0; k < corpus.Length; k++)
            {
                // Compute the words in the document.
                string document = corpus[k];
                string[] words = Regex.Split(document, @"\W+");

                // Count the number of times each word occurs in the document.
                int count = words.Count(word => word.Equals(str, StringComparison.OrdinalIgnoreCase));
                tf[k] = count / words.Length;
                df += count > 0 ? 1 : 0;
            }

            // Calculate tf-idf for each document.
            double idf = Math.Log(corpus.Length / df);
            double[] tfidf = new double[corpus.Length];
            for (int k = 0; k < corpus.Length; k++)
                tfidf[k] = tf[k] * idf;
            return tfidf;
        }
    }
}