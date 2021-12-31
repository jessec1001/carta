using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CartaCore.Utilities
{
    /// <summary>
    /// Provides utility methods for working with dictionaries.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Converts a dictionary into a typed value.
        /// </summary>
        /// <param name="fields">The dictionary of fields.</param>
        /// <param name="type">The type of value.</param>
        /// <returns>The converted value.</returns>
        public static object AsTyped(this Dictionary<string, object> fields, Type type)
        {
            // Initialize the value.
            object value = Activator.CreateInstance(type);

            // Foreach field in the dictionary, find a corresponding property in the value if it exists and set it.
            foreach (KeyValuePair<string, object> entry in fields)
            {
                PropertyInfo property = type.GetProperty(
                    entry.Key,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.IgnoreCase
                );

                if (property is null) continue;
                else property.SetValue(value, entry.Value);
            }
            return value;
        }
        /// <summary>
        /// Converts a dictionary into a typed value.
        /// </summary>
        /// <param name="fields">The dictionary of fields.</param>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <returns>The converted value.</returns>
        public static T AsTyped<T>(this Dictionary<string, object> fields)
        {
            return (T)AsTyped(fields, typeof(T));
        }
        /// <summary>
        /// Converts a value into a field dictionary.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="type">The type of value.</param>
        /// <returns>The converted dictionary.</returns>
        public static Dictionary<string, object> AsDictionary(this object value, Type type)
        {
            // Foreach property of the type, convert to a dictionary entry.
            Dictionary<string, object> fields = type
                .GetProperties(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.IgnoreCase
                )
                .ToDictionary(
                    property => property.Name,
                    property => property.GetValue(value)
                );
            return fields;
        }
        /// <summary>
        /// Converts a value into a field dictionary.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <returns>The converted dictionary.</returns>
        public static Dictionary<string, object> AsDictionary<T>(this T value)
        {
            return AsDictionary(value, typeof(T));
        }

        private static class DictionaryHashing
        {
            /// <summary>
            /// Computes the byte-array representation of any type of object.
            /// </summary>
            /// <param name="value">The value to convert into a byte array.</param>
            /// <returns>The byte array.</returns>
            private static byte[] ComputeByteArray(object value)
            {
                // We will use this value type for special cases of the type such as nullables, arrays, etc.
                Type valueType = value.GetType();

                // We handle null values as an empty byte array.
                if (value is null) return Array.Empty<byte>();

                // Since we have checked for null equality, we can now check if we have a non-null nullable value type.
                Type nullableType = Nullable.GetUnderlyingType(valueType);
                if (nullableType is not null)
                    return ComputeByteArray(valueType.GetProperty(nameof(Nullable<byte>.Value)).GetValue(value));

                // Check for array type.
                if (valueType.IsArray)
                {
                    // Get the array as an array of arrays of bytes.
                    Array arrayValue = (Array)value;
                    int byteLength = 0;
                    byte[][] bytes = new byte[arrayValue.Length][];
                    for (int k = 0; k < arrayValue.Length; k++)
                    {
                        bytes[k] = ComputeByteArray(arrayValue.GetValue(k));
                        byteLength += bytes[k].Length;
                    }

                    // Copy the byte arrays into a flat, sequential byte array.
                    byte[] byteArray = new byte[byteLength];
                    int byteIndex = 0;
                    for (int k = 0; k < arrayValue.Length; k++)
                    {
                        Buffer.BlockCopy(bytes[k], 0, byteArray, byteIndex, bytes[k].Length);
                        byteIndex += bytes[k].Length;
                    }
                    return bytes.SelectMany(bytes => bytes).ToArray();
                }

                return value switch
                {
                    string valueTyped => Encoding.ASCII.GetBytes(valueTyped),
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
            /// Creates a hash of a key in the field dictionary.
            /// </summary>
            /// <param name="hasher">The hashing algorithm to use.</param>
            /// <param name="key">The key to hash.</param>
            /// <returns>The byte-array representation of the hash.</returns>
            public static async Task<byte[]> ComputeKeyHashAsync(HashAlgorithm hasher, string key)
            {
                byte[] byteArray = Encoding.ASCII.GetBytes(key);
                using MemoryStream stream = new(byteArray);
                return await hasher.ComputeHashAsync(stream);
            }
            /// <summary>
            /// Computes a hash of a value in the field dictionary.
            /// </summary>
            /// <param name="hasher">The hashing algorithm to use.</param>
            /// <param name="value">The value to hash.</param>
            /// <returns>The byte-array representation of the hash.</returns>
            public static async Task<byte[]> ComputeValueHashAsync(HashAlgorithm hasher, object value)
            {
                byte[] byteArray = ComputeByteArray(value);
                using MemoryStream stream = new(byteArray);
                return await hasher.ComputeHashAsync(stream);
            }
        }

        /// <summary>
        /// Computes the deterministic hash of this dictionary. Deterministic in this instance means that if the keys
        /// and their corresponding values are the same as in another dictionary (regardless of order), the hash will be
        /// identical.
        /// </summary>
        /// <returns>The byte array representing the hash.</returns>
        public static async Task<byte[]> ComputeHashAsync(this Dictionary<string, object> fields, HashAlgorithm hasher = null)
        {
            // Assign the hasher if it is null.
            bool unmanaged = hasher is null;
            if (unmanaged) hasher = MD5.Create();

            // Create the sorter function.
            IComparer<byte[]> comparer = new ArrayComparer<byte>();

            // Compute the hashes of all of the keys and values.
            byte[][] keyHashes = new byte[fields.Count][];
            byte[][] valueHashes = new byte[fields.Count][];
            int index = 0;
            foreach (KeyValuePair<string, object> pair in fields)
            {
                // TODO: Improve efficiency by computing all hashing asynchronously.
                keyHashes[index] = await DictionaryHashing.ComputeKeyHashAsync(hasher, pair.Key);
                valueHashes[index] = await DictionaryHashing.ComputeValueHashAsync(hasher, pair.Value);
                index++;
            }

            // We sort the associated arrays of hashes by their key hashes. 
            Array.Sort(keyHashes, valueHashes, comparer);

            // We combine all of the hashes into a single hash.
            int hashSize = hasher.HashSize / 8;
            byte[] totalHash = new byte[2 * fields.Count * hashSize];
            for (int itemIndex = 0; itemIndex < fields.Count; itemIndex++)
            {
                for (int byteIndex = 0; byteIndex < hashSize; byteIndex++)
                {
                    totalHash[2 * itemIndex * hashSize + 0 * hashSize + byteIndex] = keyHashes[itemIndex][byteIndex];
                    totalHash[2 * itemIndex * hashSize + 1 * hashSize + byteIndex] = valueHashes[itemIndex][byteIndex];
                }
            }

            // Return the hash of the total hash.
            using MemoryStream stream = new(totalHash);
            byte[] hash = await hasher.ComputeHashAsync(stream);

            // Dispose of the hasher if it was created by this method.
            if (unmanaged) hasher.Dispose();
            return hash;
        }
    }
}