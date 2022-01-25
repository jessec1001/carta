using System;
using System.Collections.Generic;
using System.Reflection;
using CartaCore.Typing.Attributes;
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
            if (converter.TryConvert(fields.GetType(), type, fields, out object result)) return result;
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
            if (converter.TryConvert(type, typeof(Dictionary<string, object>), value, out object result))
                return result as Dictionary<string, object>;
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

        /// <summary>
        /// Modifies an existing type converter by applying a collection of attributes.
        /// Notice that this method is safe to rely on from external code because we can construct attributes without
        /// needing a reference to member information.
        /// </summary>
        /// <param name="context">The type converter context to modify.</param>
        /// <param name="attributes">The attributes to apply.</param>
        /// <returns>A modified type converter context.</returns>
        public static TypeConverterContext ApplyAttributes(
            this TypeConverterContext context,
            IEnumerable<Attribute> attributes
        )
        {
            // Default for a null context.
            if (context == null) context = new TypeConverterContext();

            // Construct a new context with any type converter attributes applied to the stack.
            List<TypeConverter> converters = new(context.TypeConverters);
            foreach (Attribute attribute in attributes)
            {
                if (attribute is ITypeConverterAttribute typeConverterAttribute)
                    typeConverterAttribute.ApplyConverter(converters);
            }
            return new TypeConverterContext(converters.ToArray());
        }
    }
}