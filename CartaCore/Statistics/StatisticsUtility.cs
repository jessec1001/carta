using System;

namespace CartaCore.Statistics
{
    /// <summary>
    /// Contains some simple utilities for computing statistical quantities.
    /// </summary>
    public static class StatisticsUtility
    {
        /// <summary>
        /// Computes the mean of a list of values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns>The mean.</returns>
        public static double ComputeMean(double[] values)
        {
            double sum = 0.0;
            foreach (double value in values)
                sum += value;
            return sum / values.Length;
        }
        /// <summary>
        /// Computes the median of a list of values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns>The median.</returns>
        public static double ComputeMedian(double[] values)
        {
            // Sort the array of values.
            double[] sorted = (double[])values.Clone();
            Array.Sort(sorted);

            // If the number of values is zero, return zero.
            // If the number of values is even, return the mean of the two middle values.
            // If the number of values is odd, return the middle value.
            if (sorted.Length == 0)
                return 0.0;
            if (sorted.Length % 2 == 0)
                return (sorted[sorted.Length / 2 - 1] + sorted[sorted.Length / 2]) / 2.0;
            return sorted[sorted.Length / 2];
        }
        /// <summary>
        /// Computes a specific sample moment of a list of values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="order">The order of the moment.</param>
        /// <returns>The moment.</returns>
        public static double ComputeMoment(double[] values, double order)
        {
            double mean = ComputeMean(values);
            double sumDiffs = 0.0;
            foreach (double value in values)
                sumDiffs += Math.Pow(value - mean, order);
            return sumDiffs / values.Length;
        }
        /// <summary>
        /// Computes a specific normalized sample moment of a list of values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="order">The order of the moment.</param>
        /// <returns>The normalized moment.</returns>
        public static double ComputeNormalizedMoment(double[] values, double order)
        {
            double moment = ComputeMoment(values, order);
            double variance = ComputeMoment(values, 2);
            return moment / Math.Pow(variance, order / 2);
        }
        /// <summary>
        /// Computes the Jarque-Bera test statistic (Chi-squared distributed) for a list of values. 
        /// </summary>
        /// <param name="values">The values,</param>
        /// <returns>The Jarque-Bera test statistic.</returns>
        public static double ComputeJarqueBera(double[] values)
        {
            double skewness = ComputeNormalizedMoment(values, 3);
            double kurtosis = ComputeNormalizedMoment(values, 4);
            return values.Length / 6 * (Math.Pow(skewness, 2) + Math.Pow(kurtosis - 3, 2) / 4);
        }
    }
}