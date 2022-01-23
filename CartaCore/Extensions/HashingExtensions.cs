using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CartaCore.Extensions.Array;

namespace CartaCore.Extensions.Hashing
{
    /// <summary>
    /// Extensions provided for computing byte representations of objects and hashing values.
    /// </summary>
    public static class HashingExtensions
    {
        /// <summary>
        /// Computes the byte-array representation of any type of object.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The byte array.</returns>
        public static byte[] ComputeByteArray(this object value)
        {
            // We handle null values an empty array.
            if (value is null) return System.Array.Empty<byte>();

            // We check for a nullable type (this is not null because of our previous check).
            Type valueType = value.GetType();
            Type nullableType = Nullable.GetUnderlyingType(valueType);
            if (nullableType is not null)
                return ComputeByteArray(nullableType.GetProperty(nameof(Nullable<byte>.Value)).GetValue(value));

            // Check for an array type.
            if (valueType.IsArray)
            {
                // We get the array as a value.
                System.Array arrayValue = value as System.Array;
                int bytesLength = 0;
                byte[][] bytes = new byte[arrayValue.Length][];
                for (int k = 0; k < arrayValue.Length; k++)
                {
                    bytes[k] = ComputeByteArray(arrayValue.GetValue(k));
                    bytesLength += bytes[k].Length;
                }

                // Copy the byte arrays into a flat, sequential byte array.
                byte[] bytesArray = new byte[bytesLength];
                int bytesIndex = 0;
                for (int k = 0; k < arrayValue.Length; k++)
                {
                    System.Array.Copy(bytes[k], 0, bytesArray, bytesIndex, bytes[k].Length);
                    bytesIndex += bytes[k].Length;
                }
                return bytesArray;
            }

            // Check for dictionary type.
            if (valueType.IsAssignableTo(typeof(IDictionary)))
            {
                // We get the dictionary as a value.
                IDictionary dictionaryValue = value as IDictionary;

                // Create a sorter function for sorting the dictionary to ensure deterministic behavior.
                IComparer<byte[]> comparer = new ArrayComparer<byte>();

                // Compute the byte arrays of all of the keys and values.
                byte[][] keysBytes = new byte[dictionaryValue.Count][];
                byte[][] valuesBytes = new byte[dictionaryValue.Count][];
                int index = 0;
                foreach (DictionaryEntry entry in dictionaryValue)
                {
                    keysBytes[index] = ComputeByteArray(entry.Key);
                    valuesBytes[index] = ComputeByteArray(entry.Value);
                    index++;
                }

                // We sort the associated arrays of bytes by their keys.
                System.Array.Sort(keysBytes, valuesBytes, comparer);

                // We combine all of the bytes into a single byte array.
                int bytesLength = 0;
                for (int k = 0; k < keysBytes.Length; k++)
                    bytesLength += keysBytes[k].Length + valuesBytes[k].Length;

                byte[] bytesArray = new byte[bytesLength];
                int bytesIndex = 0;
                for (int k = 0; k < dictionaryValue.Count; k++)
                {
                    System.Array.Copy(keysBytes[k], 0, bytesArray, bytesIndex, keysBytes[k].Length);
                    bytesIndex += keysBytes[k].Length;
                    System.Array.Copy(valuesBytes[k], 0, bytesArray, bytesIndex, valuesBytes[k].Length);
                    bytesIndex += valuesBytes[k].Length;
                }
                return bytesArray;
            }

            // Check for primitive types.
            return value switch
            {
                string valueTyped => Encoding.UTF8.GetBytes(valueTyped),
                bool valueTyped => BitConverter.GetBytes(valueTyped),
                char valueTyped => BitConverter.GetBytes(valueTyped),
                short valueTyped => BitConverter.GetBytes(valueTyped),
                ushort valueTyped => BitConverter.GetBytes(valueTyped),
                int valueTyped => BitConverter.GetBytes(valueTyped),
                uint valueTyped => BitConverter.GetBytes(valueTyped),
                long valueTyped => BitConverter.GetBytes(valueTyped),
                ulong valueTyped => BitConverter.GetBytes(valueTyped),
                float valueTyped => BitConverter.GetBytes(valueTyped),
                double valueTyped => BitConverter.GetBytes(valueTyped),
                _ => BitConverter.GetBytes(value.GetHashCode()),
            };
        }

        /// <summary>
        /// Creates a hash of the specified byte array using an optionally specified hashing algorithm.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <param name="hasher">The hashing algorithm. If not specified, defaults to SHA256.</param>
        /// <returns>The hash byte array.</returns>
        public static byte[] ComputeHash(this byte[] bytes, HashAlgorithm hasher = null)
        {
            // By default, we use SHA-256.
            bool managed = hasher is null;
            if (managed) hasher = SHA256.Create();

            // We compute the hash.
            byte[] hash = hasher.ComputeHash(bytes);

            // If we used a managed hasher, we dispose it.
            if (managed) hasher.Dispose();

            return hash;
        }
        /// <summary>
        /// Creates a hash of the specified byte array using and optionally specified hashing algorithm asynchronously.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <param name="hasher">The hashing algorithm. If not specified, defaults to SHA256.</param>
        /// <returns>The hash byte array.</returns>
        public static async Task<byte[]> ComputeHashAsync(this byte[] bytes, HashAlgorithm hasher = null)
        {
            // By default, we use SHA-256.
            bool managed = hasher is null;
            if (managed) hasher = SHA256.Create();

            // We compute the hash.
            using MemoryStream stream = new(bytes);
            byte[] hash = await hasher.ComputeHashAsync(stream);

            // If we used a managed hasher, we dispose it.
            if (managed) hasher.Dispose();

            return hash;
        }
        /// <summary>
        /// Creates a hash of the specified stream using an optionally specified hashing algorithm asynchronously.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="hasher">The hashing algorithm. If not specified, defaults to SHA256.</param>
        /// <returns>The hash byte array.</returns>
        public static async Task<byte[]> ComputeHashAsync(this Stream stream, HashAlgorithm hasher = null)
        {
            // By default, we use SHA-256.
            bool managed = hasher is null;
            if (managed) hasher = SHA256.Create();

            // We compute the hash.
            byte[] hash = await hasher.ComputeHashAsync(stream);

            // If we used a managed hasher, we dispose it.
            if (managed) hasher.Dispose();

            return hash;
        }
    }
}