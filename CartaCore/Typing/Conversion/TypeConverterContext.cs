using System;

namespace CartaCore.Typing.Conversion
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
        private TypeConverter[] TypeConverters { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeConverterContext"/> class with the given converters.
        /// </summary>
        /// <param name="typeConverters">The converters to use. Should be speicifed in the order of application.</param>
        public TypeConverterContext(params TypeConverter[] typeConverters)
        {
            TypeConverters = typeConverters;
        }

        /// <inheritdoc />
        public override bool CanConvert(Type sourceType, Type targetType, TypeConverterContext context = null)
        {
            // Check the trivial case.
            if (sourceType.IsAssignableTo(targetType)) return true;

            // We check if any of the contained converters can convert the given types.
            // Notice that we pass this context to the contained converters.
            foreach (TypeConverter converter in TypeConverters)
                if (converter.CanConvert(sourceType, targetType, this)) return true;
            return false;
        }
        /// <inheritdoc />
        public override bool TryConvert(Type type, object value, out object result, TypeConverterContext context = null)
        {
            // Check the trivial case.
            if (value.GetType().IsAssignableTo(type))
            {
                result = value;
                return true;
            }

            // Try each of the contained converters until one succeeds.
            // Notice that we pass this context to the contained converters.
            foreach (TypeConverter converter in TypeConverters)
            {
                // If a converter throws an error, we ignore it.
                try
                {
                    // By convention, we need to check if the converter can convert the given type.
                    if (!converter.CanConvert(value.GetType(), type, this)) continue;
                    if (converter.TryConvert(type, value, out result, this)) return true;
                }
                catch { }
            }

            // If none of the contained converters succeeded, we return false.
            result = null;
            return false;
        }

        /// <summary>
        /// The default type converter that is applicable in most scenarios.
        /// </summary>
        public static TypeConverterContext Default
        {
            get
            {
                return new TypeConverterContext(
                    new EnumTypeConverter(),
                    new NumericTypeConverter(),
                    new ArrayTypeConverter(),
                    new DictionaryTypeConverter()
                );
            }
        }
    }
}