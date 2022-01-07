using System;

namespace CartaCore.Typing.Conversion
{
    /// <summary>
    /// A general base class for type converters.
    /// </summary>
    public abstract class TypeConverter
    {
        /// <summary>
        /// Determines whether this instance can convert from a source type to a target type.
        /// </summary>
        /// <param name="sourceType">The source type.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="context">The conversion context.</param>
        /// <returns><c>true</c> if the conversion is possible; otherwise <c>false</c>.</returns>
        public virtual bool CanConvert(Type sourceType, Type targetType, TypeConverterContext context = null)
            => true;
        /// <summary>
        /// Determines whether this instance can convert from a source type to a target type.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <param name="context">The conversion context.</param>
        /// <returns><c>true</c> if the conversion is possible; otherwise <c>false</c>.</returns>
        public bool CanConvert<TSource, TTarget>(TypeConverterContext context = null)
            => CanConvert(typeof(TSource), typeof(TTarget), context);

        /// <summary>
        /// Tries to convert the given value to the given type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        /// <param name="result">The converted result if successful.</param>
        /// <param name="context">The conversion context.</param>
        /// <returns><c>true</c> if the conversion was successful; otherwise, <c>false</c>.</returns>
        public virtual bool TryConvert(Type type, object value, out object result, TypeConverterContext context = null)
        {
            if (CanConvert(value.GetType(), type))
            {
                result = Convert.ChangeType(value, type);
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }
        /// <summary>
        /// Tries to convert the given value to the given type.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="result">The converted result if successful.</param>
        /// <param name="context">The conversion context.</param>
        /// <typeparam name="T">The type</typeparam>
        /// <returns><c>true</c> if the conversion was successful; otherwise, <c>false</c>.</returns>
        public bool TryConvert<T>(object value, out T result, TypeConverterContext context = null)
        {
            bool success = TryConvert(typeof(T), value, out object resultObject, context);
            result = success ? (T)resultObject : default;
            return success;
        }
    }
}