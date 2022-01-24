using System;

namespace CartaCore.Typing.Conversion
{
    /// <summary>
    /// Converts values between a string and an enumeration.
    /// </summary>
    public class EnumTypeConverter : TypeConverter
    {
        /// <inheritdoc />
        public override bool CanConvert(Type sourceType, Type targetType, TypeConverterContext context = null)
        {
            if (sourceType.IsAssignableTo(typeof(string)) && targetType.IsEnum) return true;
            if (targetType.IsAssignableTo(typeof(string)) && sourceType.IsEnum) return true;
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
            // Check if the source and target types are compatible.
            if (!CanConvert(sourceType, targetType, context))
            {
                output = null;
                return false;
            }

            // Perform the conversion.
            if (input is string representation)
            {
                output = Enum.Parse(targetType, representation);
                return true;
            }
            else
            {
                output = Enum.GetName(targetType, input);
                return true;
            }
        }
    }
}