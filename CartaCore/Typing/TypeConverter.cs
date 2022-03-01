using System;

namespace CartaCore.Typing
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
            => context is not null && context.CanConvert(sourceType, targetType);
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
        /// Tries to convert the given value from a source type to a target type.
        /// Implementations of this method should use <see cref="CanConvert"/> to determine if the conversion is
        /// possible internally.
        /// </summary>
        /// <param name="sourceType">The source type.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="input">The value to be converted.</param>
        /// <param name="output">The value that has been converted.</param>
        /// <param name="context">The conversion context.</param>
        /// <returns><c>true</c> if the conversion was successful; otherwise, <c>false</c>.</returns>
        public virtual bool TryConvert(Type sourceType, Type targetType, in object input, out object output, TypeConverterContext context = null)
        {
            if (CanConvert(sourceType, targetType, context) && context is not null)
            {
                bool success = context.TryConvert(sourceType, targetType, in input, out output);
                return success;
            }
            else
            {
                output = default;
                return false;
            }
        }
        /// <summary>
        /// Tries to convert the given value from a source type to a target type.
        /// Implementations of this method should use <see cref="CanConvert"/> to determine if the conversion is
        /// possible internally.
        /// </summary>
        /// <param name="input">The value to be converted.</param>
        /// <param name="output">The value that has been converted.</param>
        /// <param name="context">The conversion context.</param>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <returns><c>true</c> if the conversion was successful; otherwise, <c>false</c>.</returns>
        public bool TryConvert<TSource, TTarget>(in TSource input, out TTarget output, TypeConverterContext context = null)
        {
            // Prepare the input and output values.
            object untypedInput = input;
            object untypedOutput;

            bool success = TryConvert(typeof(TSource), typeof(TTarget), in untypedInput, out untypedOutput, context);
            output = success ? (TTarget)untypedOutput : default;
            return success;
        }
    }
}