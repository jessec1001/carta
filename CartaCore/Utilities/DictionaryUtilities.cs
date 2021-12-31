using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CartaCore.Utilities
{
    /// <summary>
    /// Provides utility methods for working with dictionaries.
    /// </summary>
    public static class DictionaryUtilities
    {
        /// <summary>
        /// Converts a dictionary into a typed value.
        /// </summary>
        /// <param name="fields">The dictionary of fields.</param>
        /// <param name="type">The type of value.</param>
        /// <returns>The converted value.</returns>
        public static object AsTyped(Dictionary<string, object> fields, Type type)
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
        public static T AsTyped<T>(Dictionary<string, object> fields)
        {
            return (T)AsTyped(fields, typeof(T));
        }
        /// <summary>
        /// Converts a value into a field dictionary.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="type">The type of value.</param>
        /// <returns>The converted dictionary.</returns>
        public static Dictionary<string, object> AsDictionary(object value, Type type)
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
        public static Dictionary<string, object> AsDictionary<T>(T value)
        {
            return AsDictionary(value, typeof(T));
        }
    }
}