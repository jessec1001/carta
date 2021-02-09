using System;
using System.Text.RegularExpressions;

namespace CartaCore.Statistics
{
    /// <summary>
    /// Represents a discrete uniform distribution where samples are generated from a minimum value to a maximum value
    /// inclusively.
    /// </summary>
    public class DiscreteUniformDistribution : IIntegerDistribution
    {
        /// <summary>
        /// Gets the minimum value.
        /// </summary>
        /// <value>The minimum (inclusive) value in the support.</value>
        public int Minimum { get; protected set; }
        /// <summary>
        /// Gets the maximum value.
        /// </summary>
        /// <value>The maximum (inclusive) value in the support.</value>
        public int Maximum { get; protected set; }

        /// <summary>
        /// Initializes an instance of the <see cref="DiscreteUniformDistribution"/> class with specified maximum and
        /// minimum values.
        /// </summary>
        /// <param name="min">The minimum (inclusive) value.</param>
        /// <param name="max">The maximum (inclusive) value.</param>
        public DiscreteUniformDistribution(int min, int max)
        {
            // Check that the maximum and minimum values were ordered correctly.
            if (max < min)
                throw new ArgumentOutOfRangeException(nameof(max), max, "Maximum value was less than minimum value.");

            Minimum = min;
            Maximum = max;
        }
        /// <summary>
        /// Initializes an instance of the <see cref="DiscreteUniformDistribution"/> class with a specified maximum
        /// value and a minimum value of zero.
        /// </summary>
        /// <param name="max">The maximum (inclusive) value.</param>
        public DiscreteUniformDistribution(int max)
            : this(0, max) { }

        /// <inheritdoc />
        public int Sample(Random random)
        {
            return random.Next(Minimum, Maximum + 1);
        }
        /// <inheritdoc />
        public int Sample(CompoundRandom random)
        {
            return random.NextInt(Minimum, Maximum + 1);
        }

        /// <inheritdoc />
        public double Mean => (Minimum + Maximum) / 2.0;
        /// <inheritdoc />
        public double Variance
        {
            get
            {
                int count = Maximum - Minimum + 1;
                return (count * count - 1) / 12.0;
            }
        }

        // We create a parser regular expression to help parse text into distributions.
        private static Regex ParserRegex = new Regex
        (
            @"DU\(\s*(?<a>\d+)\s*(?:,(?<b>\d+)\s*)?\)",
            RegexOptions.Compiled |
            RegexOptions.IgnoreCase
        );
        /// <summary>
        /// Converts the string representation of a <see cref="DiscreteUniformDistribution"/> into its object
        /// equivalent. A return value indicates whether the operation succeeded.
        /// </summary>
        /// <param name="source">The source string representation.</param>
        /// <param name="dist">The output distribution. Will be null if operation failed.</param>
        /// <returns><c>true</c> if the operation succeeded; otherwise <c>false</c>.</returns>
        public static bool TryParse(string source, out DiscreteUniformDistribution dist)
        {
            // Set the distribution to null until it can be parsed.
            dist = null;

            // Check if the distribution matches our regular expression.
            Match match = ParserRegex.Match(source);
            if (!match.Success) return false;

            // Try to get our parameters from the match.
            int a = 0, b = 0;
            if (match.Groups.ContainsKey("a"))
                int.TryParse(match.Groups["a"].Value, out a);
            if (match.Groups.ContainsKey("b"))
                int.TryParse(match.Groups["b"].Value, out b);

            // Return the new distribution.
            dist = new DiscreteUniformDistribution(a, b);
            return true;
        }
        /// <inheritdoc />
        public override string ToString()
        {
            return $"DU({Minimum}, {Maximum})";
        }
    }
}