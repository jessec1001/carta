using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CartaCore.Extensions.Statistics
{
    /// <summary>
    /// Contains some simple utilities for computing statistical quantities.
    /// </summary>
    public static class StatisticsExtensions
    {
        /// <summary>
        /// Computes the median of a list of values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns>The median.</returns>
        private static double ComputeMedian(this double[] values)
        {
            // Sort the array.
            Array.Sort(values);

            // If the number of values is zero, return zero.
            // If the number of values is even, return the mean of the two middle values.
            // If the number of values is odd, return the middle value.
            if (values.Length == 0)
                return 0.0;
            if (values.Length % 2 == 0)
                return (values[values.Length / 2 - 1] + values[values.Length / 2]) / 2.0;
            return values[values.Length / 2];
        }
        /// <summary>
        /// Computes the median of a list of values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns>The median.</returns>
        public static double ComputeMedian(this IEnumerable<double> values)
        {
            // Get an array of values.
            double[] arrValues = values.ToArray();
            return arrValues.ComputeMedian();
        }
        /// <summary>
        /// Computes the median of a list of values asynchronously.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns>The median.</returns>
        public static async Task<double> ComputeMedianAsync(this IAsyncEnumerable<double> values)
        {
            // Get an array of values.
            double[] arrValues = await values.ToArrayAsync();
            return arrValues.ComputeMedian();
        }
        /// <summary>
        /// Computes a specific sample raw moment of a list of values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="order">The order of the moment.</param>
        /// <returns>The raw moment and the count of values in a tuple.</returns>
        public static (double, int) ComputeRawMoment(this IEnumerable<double> values, double order)
        {
            int count = 0;
            double sum = 0.0;
            foreach (double value in values)
            {
                count++;
                sum += Math.Pow(value, order);
            }
            return (sum / count, count);
        }
        /// <summary>
        /// Computes a specific sample raw moment of a list of values asynchronously.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="order">The order of the moment.</param>
        /// <returns>The raw moment and the count of values in a tuple.</returns>
        public static async Task<(double, int)> ComputeRawMomentAsync(this IAsyncEnumerable<double> values, double order)
        {
            int count = 0;
            double sum = 0.0;
            await foreach (double value in values)
            {
                count++;
                sum += Math.Pow(value, order);
            }
            return (sum / count, count);
        }
        /// <summary>
        /// Computes a specific sample central moment of a list of values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="order">The order of the moment.</param>
        /// <returns>The central moment and the count of values in a tuple.</returns>
        public static (double, int) ComputeCentralMoment(this IEnumerable<double> values, int order)
        {
            // These values are accumulated over the enumeration of values.
            // The orders are the sum of values raised to the power of the order.
            int count = 0;
            double sum = 0.0;
            double[] orders = new double[order + 1];
            foreach (double value in values)
            {
                count++;
                sum += value;
                for (int i = 0; i <= order; i++)
                    orders[i] += Math.Pow(value, i);
            }

            // To compute the moment, we derived a specialized formula.
            // Moment = Sum_{i=0 to n}((n choose i) * (-1)^{n-i} * (mu)^{n-i} * E[x^{i}])
            double mean = sum / count;
            double moment = 0.0;

            // We alternate the sign of the terms instead of using exponentiation for efficiency.
            // We also incrementally compute the binomial coefficients along the n-th row of Pascal's triangle.
            // - This uses the formula (n choose i) = Product_{j=1 to k} (n - j + 1) / j.
            int sign = 1;
            int binom = 1;
            for (int i = 0; i <= order; i++)
            {
                moment += binom * sign * Math.Pow(mean, order - i) * orders[i] / count;
                sign = -sign;
                binom = binom * (order - i) / (i + 1);
            }

            return (moment, count);
        }
        /// <summary>
        /// Computes a specific sample central moment of a list of values asynchronously.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="order">The order of the moment.</param>
        /// <returns>The central moment and the count of values in a tuple.</returns>
        public static async Task<(double, int)> ComputeCentralMomentAsync(this IAsyncEnumerable<double> values, int order)
        {
            // These values are accumulated over the enumeration of values.
            // The orders are the sum of values raised to the power of the order.
            int count = 0;
            double sum = 0.0;
            double[] orders = new double[order + 1];
            await foreach (double value in values)
            {
                count++;
                sum += value;
                for (int i = 0; i <= order; i++)
                    orders[i] += Math.Pow(value, i);
            }

            // To compute the moment, we derived a specialized formula.
            // Moment = Sum_{i=0 to n}((n choose i) * (-1)^{n-i} * (mu)^{n-i} * E[x^{i}])
            double mean = sum / count;
            double moment = 0.0;

            // We alternate the sign of the terms instead of using exponentiation for efficiency.
            // We also incrementally compute the binomial coefficients along the n-th row of Pascal's triangle.
            // - This uses the formula (n choose i) = Product_{j=1 to k} (n - j + 1) / j.
            int sign = 1;
            int binom = 1;
            for (int i = 0; i <= order; i++)
            {
                moment += binom * sign * Math.Pow(mean, order - i) * orders[i] / count;
                sign = -sign;
                binom = binom * (order - i) / (i + 1);
            }

            return (moment, count);
        }
        /// <summary>
        /// Computes a specific normalized sample moment of a list of values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="order">The order of the moment.</param>
        /// <returns>The normalized moment and the count of values in a tuple.</returns>
        public static (double, int) ComputeNormalizedMoment(this IEnumerable<double> values, int order)
        {
            // These values are accumulated over the enumeration of values.
            // The orders are the sum of values raised to the power of the order.
            int count = 0;
            double sum = 0.0;
            double sumSquares = 0.0;
            double[] orders = new double[order + 1];
            foreach (double value in values)
            {
                count++;
                sum += value;
                sumSquares += value * value;
                for (int i = 0; i <= order; i++)
                    orders[i] += Math.Pow(value, i);
            }

            // To compute the moment, we derived a specialized formula.
            // Moment = Sum_{i=0 to n}((n choose i) * (-1)^{n-i} * (mu)^{n-i} * E[x^{i}])
            double mean = sum / count;
            double normalizer = Math.Pow((sumSquares / count) - (mean * mean), order / 2.0);
            double moment = 0.0;

            // We alternate the sign of the terms instead of using exponentiation for efficiency.
            // We also incrementally compute the binomial coefficients along the n-th row of Pascal's triangle.
            // - This uses the formula (n choose i) = Product_{j=1 to k} (n - j + 1) / j.
            int sign = 1;
            int binom = 1;
            for (int i = order; i >= 0; i--)
            {
                moment += binom * sign * Math.Pow(mean, order - i) * orders[i] / count;
                sign = -sign;
                binom = binom * i / (order - i + 1);
            }

            return (moment / normalizer, count);
        }
        /// <summary>
        /// Computes a specific normalized sample moment of a list of values asynchronously.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="order">The order of the moment.</param>
        /// <returns>The normalized moment and the count of values in a tuple.</returns>
        public static async Task<(double, int)> ComputeNormalizedMomentAsync(this IAsyncEnumerable<double> values, int order)
        {
            // These values are accumulated over the enumeration of values.
            // The orders are the sum of values raised to the power of the order.
            int count = 0;
            double sum = 0.0;
            double sumSquares = 0.0;
            double[] orders = new double[order + 1];
            await foreach (double value in values)
            {
                count++;
                sum += value;
                sumSquares += value * value;
                for (int i = 0; i <= order; i++)
                    orders[i] += Math.Pow(value, i);
            }

            // To compute the moment, we derived a specialized formula.
            // Moment = Sum_{i=0 to n}((n choose i) * (-1)^{n-i} * (mu)^{n-i} * E[x^{i}])
            double mean = sum / count;
            double normalizer = Math.Pow((sumSquares / count) - (mean * mean), order / 2.0);
            double moment = 0.0;

            // We alternate the sign of the terms instead of using exponentiation for efficiency.
            // We also incrementally compute the binomial coefficients along the n-th row of Pascal's triangle.
            // - This uses the formula (n choose i) = Product_{j=1 to k} (n - j + 1) / j.
            int sign = 1;
            int binom = 1;
            for (int i = 0; i <= order; i++)
            {
                moment += binom * sign * Math.Pow(mean, order - i) * orders[i] / count;
                sign = -sign;
                binom = binom * (order - i) / (i + 1);
            }

            return (moment / normalizer, count);
        }

        /// <summary>
        /// Computes the Jarque-Bera test statistic (Chi-squared distributed) for a list of values. 
        /// </summary>
        /// <param name="values">The values,</param>
        /// <returns>The Jarque-Bera test statistic.</returns>
        public static double ComputeJarqueBera(this double[] values)
        {
            (double skewness, _) = ComputeNormalizedMoment(values, 3);
            (double kurtosis, _) = ComputeNormalizedMoment(values, 4);
            return values.Length / 6 * (Math.Pow(skewness, 2) + Math.Pow(kurtosis - 3, 2) / 4);
        }
    }
}