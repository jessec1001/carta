using System;
using System.Collections.Generic;
using System.Text;

namespace CartaCore.Extensions.Arrays
{
    /// <summary>
    /// Extensions provided for working with arrays with common applications.
    /// </summary>
    public static class ArraysExtensions
    {
        /// <summary>
        /// Converts an array of bytes into a hexadecimal string.
        /// </summary>
        /// <param name="bytes">The array of bytes.</param>
        /// <returns>The hexadecimal string.</returns>
        public static string ToHexadecimalString(this byte[] bytes)
        {
            StringBuilder stringBuilder = new(bytes.Length * 2);
            foreach (byte b in bytes)
                stringBuilder.AppendFormat("{0:x2}", b);
            return stringBuilder.ToString();
        }
        /// <summary>
        /// Converts an array of bytes into a binary string.
        /// </summary>
        /// <param name="bytes">The array of bytes.</param>
        /// <returns>The binary string.</returns>
        public static string ToBinaryString(this byte[] bytes)
        {
            StringBuilder stringBuilder = new(bytes.Length * 8);
            foreach (byte b in bytes)
                stringBuilder.AppendFormat("{0:b}", b);
            return stringBuilder.ToString();
        }
    }

    /// <summary>
    /// A comparer for arrays of comparable types. Values will be compared lexigraphically first and by length second.
    /// </summary>
    public class ArrayComparer<T> : IComparer<T[]>
        where T : IComparable<T>
    {
        /// <inheritdoc />
        public int Compare(T[] x, T[] y)
        {
            int length = Math.Min(x.Length, y.Length);
            for (int k = 0; k < length; k++)
            {
                int result = x[k].CompareTo(y[k]);
                if (result != 0) return result;
            }
            return x.Length.CompareTo(y.Length);
        }
    }
}