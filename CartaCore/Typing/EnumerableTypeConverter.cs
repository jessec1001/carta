using System;
using System.Collections.Generic;
using System.Linq;

namespace CartaCore.Typing
{
    /// <summary>
    /// Converts from one enumerable type to another.
    /// </summary>
    public class EnumerableTypeConverter : TypeConverter
    {
        /// <inheritdoc />
        public override bool CanConvert(Type sourceType, Type targetType, TypeConverterContext context = null)
        {
            // Types are only convertible if they are both enumerable.
            bool enumerable = (
                sourceType.IsGenericType &&
                sourceType.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                targetType.IsGenericType &&
                targetType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
            );
            if (!enumerable) return false;

            // Check if the element types are compatible.
            Type sourceElementType = sourceType.GetGenericArguments().First();
            Type targetElementType = targetType.GetGenericArguments().First();
            if (context is not null) return context.CanConvert(sourceElementType, targetElementType);
            else return false;
        }
        /// <inheritdoc />
        public override bool TryConvert(
            Type sourceType,
            Type targetType,
            in object input,
            out object output,
            TypeConverterContext context = null)
        {
            // Check that the source and target types are compatible.
            if (!CanConvert(sourceType, targetType, context))
            {
                output = null;
                return false;
            }

            // Get the enumerable types.
            Type sourceElementType = sourceType.GetGenericArguments().First();
            Type targetElementType = targetType.GetGenericArguments().First();

            // Create a new target enumerable of the given element type.
            try
            {
                output = Enumerate(sourceElementType, targetElementType, (IEnumerable<object>)input, context);
                return true;
            }
            catch
            {
                output = null;
                return false;
            }
        }

        /// <summary>
        /// Enumerates over a enumeration of values and converts each value to the given type. 
        /// </summary>
        /// <param name="sourceElementType">The source element type.</param>
        /// <param name="targetElementType">The target element type.</param>
        /// <param name="values">The enumeration of values.</param>
        /// <param name="context">The context for type conversion.</param>
        /// <returns>The converted enumeration.</returns>
        private static IEnumerable<object> Enumerate(
            Type sourceElementType,
            Type targetElementType,
            IEnumerable<object> values,
            TypeConverterContext context = null)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            foreach (object value in values)
            {
                if (!context.TryConvert(sourceElementType, targetElementType, value, out object converted))
                    throw new InvalidCastException();
                yield return converted;
            }
        }
    }
}