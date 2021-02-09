using System;

namespace CartaCore.Statistics
{
    /// <summary>
    /// Represents a distribution that has a support that is a subset of the integers.
    /// </summary>
    public interface IIntegerDistribution
    {
        /// <summary>
        /// Generates a random sample from the distribution.
        /// </summary>
        /// <param name="random">A random number generator.</param>
        /// <returns>The random sample.</returns>
        int Sample(Random random);
        /// <summary>
        /// Generates a random sample from the distribution.
        /// </summary>
        /// <param name="random">A random number generator.</param>
        /// <returns>The random sample.</returns>
        int Sample(CompoundRandom random);

        /// <summary>
        /// Gets the mean value of the distribution.
        /// </summary>
        /// <value>The distribution mean.</value>
        double Mean { get; }
        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        /// <value>The distribution variance.</value>
        double Variance { get; }

        /// <summary>
        /// Converts the string representation of a <see cref="IIntegerDistribution"/> into its object
        /// equivalent. A return value indicates whether the operation succeeded.
        /// </summary>
        /// <param name="source">The source string representation.</param>
        /// <param name="dist">The output distribution. Will be null if operation failed.</param>
        /// <returns><c>true</c> if the operation succeeded; otherwise <c>false</c>.</returns>
        static bool TryParse(string source, out IIntegerDistribution dist)
        {
            dist = null;
            if (DiscreteUniformDistribution.TryParse(source, out DiscreteUniformDistribution duDistribution))
            {
                dist = duDistribution;
                return true;
            }
            if (PoissonDistribution.TryParse(source, out PoissonDistribution poisDistribution))
            {
                dist = poisDistribution;
                return true;
            }
            return false;
        }
    }
}