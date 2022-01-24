using System;
using System.Collections.Generic;
using CartaCore.Typing.Conversion;

namespace CartaCore.Typing.Attributes
{
    /// <summary>
    /// Specifies a list of types that a property or field can take on after conversion.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class MultipleTypeAttribute : Attribute, ITypeConverterAttribute
    {
        /// <summary>
        /// Converts from a more specific type in a list to a more general type.
        /// </summary>
        private class MultipleTypeConverter : TypeConverter
        {
            private Type[] Types { get; init; }

            public MultipleTypeConverter(params Type[] types) => Types = types;

            /// <inheritdoc />
            public override bool CanConvert(Type sourceType, Type targetType, TypeConverterContext context = null)
            {
                foreach (Type intermediateType in Types)
                {
                    // Check that the following conversions are possible:
                    // 1. From the source type to the intermediate type;
                    // 2. From the intermediate type to the target type.
                    if (context is null || !context.CanConvert(sourceType, intermediateType)) continue;
                    if (context is null || !context.CanConvert(intermediateType, targetType)) continue;

                    return true;
                }
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
                // Try each of the type conversions until one succeeds. 
                foreach (Type intermediateType in Types)
                {
                    // Check that the following conversions are possible:
                    // 1. From the source type to the intermediate type;
                    // 2. From the intermediate type to the target type.
                    if (context is null || !context.CanConvert(sourceType, intermediateType)) continue;
                    if (context is null || !context.CanConvert(intermediateType, targetType)) continue;

                    // We attempt to do the conversions.
                    try
                    {
                        if (
                            context is null ||
                            !context.TryConvert(sourceType, intermediateType, in input, out object intermediateOutput)
                        ) continue;
                        if (
                            context is null ||
                            !context.TryConvert(intermediateType, targetType, in intermediateOutput, out output)
                        ) continue;
                    }
                    catch { continue; }

                    return true;
                }

                // If none of the conversions succeeded, we return false.
                output = default;
                return false;
            }
        }

        /// <summary>
        /// The types that are allowed on the object that this attribute is applied to.
        /// </summary>
        public Type[] Types { get; private init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleTypeAttribute"/> class with the specified types.
        /// </summary>
        /// <param name="types">The types to allow an object to take on.</param>
        public MultipleTypeAttribute(params Type[] types) => Types = types;

        /// <inheritdoc />
        public void ApplyConverter(IList<TypeConverter> converters)
        {
            // Append the new converter to the top of the list of converters.
            converters.Insert(0, new MultipleTypeConverter(Types));
        }
    }
}