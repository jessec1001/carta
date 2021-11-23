using System;

namespace CartaCore.Statistics
{
    /// <summary>
    /// Contains some simple utilities for computing statistical quantities.
    /// </summary>
    public static class StatisticsUtils
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
    }
}