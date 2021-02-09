using System;
using System.Text.RegularExpressions;

namespace CartaCore.Statistics
{
    /// <summary>
    /// Represents a Poisson distribution where samples are generated using the lamda rate parameter.
    /// </summary>
    public class PoissonDistribution : IIntegerDistribution
    {
        /// <summary>
        /// Gets the rate parameter of the distribution. 
        /// </summary>
        /// <value>The rate parameter.</value>
        public double Lambda { get; protected set; }

        /// <summary>
        /// Initializes an instance of the <see cref="PoissonDistribution"/> class with the specified rate parameter.
        /// </summary>
        /// <param name="lambda">The rate parameter.</param>
        public PoissonDistribution(double lambda = 1.0)
        {
            if (lambda < 0)
                throw new ArgumentOutOfRangeException(nameof(lambda), lambda, "Rate parameter must be non-negative");

            Lambda = lambda;
        }

        /// <inheritdoc />
        public int Sample(Random random)
        {
            double probability = 1.0;
            double threshold = Math.Exp(-Lambda);

            int arrivals = 0;
            while (probability >= threshold)
            {
                arrivals++;
                probability *= random.NextDouble();
            }

            return arrivals - 1;
        }
        /// <inheritdoc />
        public int Sample(CompoundRandom random)
        {
            double probability = 1.0;
            double threshold = Math.Exp(-Lambda);

            int arrivals = 0;
            while (probability >= threshold)
            {
                arrivals++;
                probability *= random.NextDouble();
            }

            return arrivals - 1;
        }

        /// <inheritdoc />
        public double Mean => Lambda;
        /// <inheritdoc />
        public double Variance => Lambda;

        // We create a parser regular expression to help parse text into distributions.
        private static Regex ParserRegex = new Regex
        (
            @"Pois\(\s*(?<lambda>\d*(?:\.\d+)?)\s*\)",
            RegexOptions.Compiled |
            RegexOptions.IgnoreCase
        );
        /// <summary>
        /// Converts the string representation of a <see cref="PoissonDistribution"/> into its object
        /// equivalent. A return value indicates whether the operation succeeded.
        /// </summary>
        /// <param name="source">The source string representation.</param>
        /// <param name="dist">The output distribution. Will be null if operation failed.</param>
        /// <returns><c>true</c> if the operation succeeded; otherwise <c>false</c>.</returns>
        public static bool TryParse(string source, out PoissonDistribution dist)
        {
            // Set the distribution to null until it can be parsed.
            dist = null;

            // Check if the distribution matches our regular expression.
            Match match = ParserRegex.Match(source);
            if (!match.Success) return false;

            // Try to get the rate parameter from the match.
            double lambda = 1.0;
            if (match.Groups.ContainsKey("lambda"))
                if (!double.TryParse(match.Groups["lambda"].Value, out lambda)) lambda = 1.0;

            // Return the new distribution.
            dist = new PoissonDistribution(lambda);
            return true;
        }
        /// <inheritdoc />
        public override string ToString()
        {
            return $"Pois({Lambda})";
        }
    }
}