using System;
using System.Collections.Generic;
using CartaCore.Typing.Conversion;

namespace CartaCore.Extensions.Typing
{
    /// <summary>
    /// Provides utility methods for working with typing and type conversions.
    /// </summary>
    public static class TypingExtensions
    {
        /// <summary>
        /// Converts a dictionary into a typed value.
        /// </summary>
        /// <param name="fields">The dictionary of fields.</param>
        /// <param name="type">The type of value.</param>
        /// <param name="context">The type conversion context. Optional.</param>
        /// <returns>The converted value.</returns>
        public static object AsTyped(
            this Dictionary<string, object> fields,
            Type type,
            TypeConverterContext context = null)
        {
            TypeConverter converter = (TypeConverter)context ?? new DictionaryTypeConverter();
            if (converter.TryConvert(type, fields, out object result)) return result;
            throw new InvalidOperationException($"Could not convert dictionary to type {type.Name}.");
        }
        /// <summary>
        /// Converts a dictionary into a typed value.
        /// </summary>
        /// <param name="fields">The dictionary of fields.</param>
        /// <param name="context">The type conversion context. Optional.</param>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <returns>The converted value.</returns>
        public static T AsTyped<T>(this Dictionary<string, object> fields, TypeConverterContext context = null)
        {
            return (T)AsTyped(fields, typeof(T), context);
        }
        /// <summary>
        /// Converts a value into a field dictionary.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="type">The type of value.</param>
        /// <param name="context">The type conversion context. Optional.</param>
        /// <returns>The converted dictionary.</returns>
        public static Dictionary<string, object> AsDictionary(
            this object value,
            Type type,
            TypeConverterContext context = null)
        {
            TypeConverter converter = (TypeConverter)context ?? new DictionaryTypeConverter();
            if (converter.TryConvert(value, out Dictionary<string, object> result)) return result;
            throw new InvalidOperationException($"Could not convert value to dictionary.");
        }
        /// <summary>
        /// Converts a value into a field dictionary.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="context">The type conversion context. Optional.</param>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <returns>The converted dictionary.</returns>
        public static Dictionary<string, object> AsDictionary<T>(this T value, TypeConverterContext context = null)
        {
            return AsDictionary(value, typeof(T), context);
        }
    }
}