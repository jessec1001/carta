using System;
using System.Collections.ObjectModel;

namespace CartaCore.Typing
{
    /// <summary>
    /// A context for type conversion. Contains all available type converters.
    /// </summary>
    public class TypeConverterContext : TypeConverter
    {
        // TODO: Adding some caching for the corresponding converters to a pair of types might make conversions faster.

        /// <summary>
        /// The internal converters that are used to convert between types.
        /// Note that these converters should be specified in the order that they are used.
        /// </summary>
        public ReadOnlyCollection<TypeConverter> TypeConverters { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeConverterContext"/> class with the given converters.
        /// </summary>
        /// <param name="typeConverters">The converters to use. Should be speicifed in the order of application.</param>
        public TypeConverterContext(params TypeConverter[] typeConverters)
        {
            TypeConverters = Array.AsReadOnly(typeConverters);
        }

        /// <inheritdoc />
        public override bool CanConvert(Type sourceType, Type targetType, TypeConverterContext context = null)
        {
            // Check the trivial case.
            if (sourceType.IsAssignableFrom(typeof(object))) return true;
            if (sourceType.IsAssignableTo(targetType)) return true;

            // We check if any of the contained converters can convert the given types.
            // Notice that we pass this context to the contained converters.
            foreach (TypeConverter converter in TypeConverters)
                if (converter.CanConvert(sourceType, targetType, this)) return true;
            return false;
        }
        /// <inheritdoc />
        public override bool TryConvert(
            Type sourceType,
            Type targetType,
            in object input,
            out object output,
            TypeConverterContext context = null)
        {
            // Check the trivial case.
            if (input is null)
            {
                output = Activator.CreateInstance(targetType);
                return true;
            }
            if (sourceType.IsAssignableTo(targetType))
            {
                output = input;
                return true;
            }

            // TODO: Make this its own type converter.
            // Check for nullable types.
            Type targetNullableType = Nullable.GetUnderlyingType(targetType);
            if (input is null && targetNullableType is not null)
            {
                output = Activator.CreateInstance(targetNullableType);
                return true;
            }

            // Try each of the contained converters until one succeeds.
            // Notice that we pass this context to the contained converters.
            foreach (TypeConverter converter in TypeConverters)
            {
                // If a converter throws an error, we ignore it.
                try { if (converter.TryConvert(sourceType, targetType, in input, out output, this)) return true; }
                catch { }
            }

            // If none of the contained converters succeeded, we return false.
            output = null;
            return false;
        }
    }
}