using System;

namespace CartaCore.Typing
{
    /// <summary>
    /// Converts values between a string and an enumeration.
    /// </summary>
    public class EnumTypeConverter : TypeConverter
    {
        /// <inheritdoc />
        public override bool CanConvert(Type sourceType, Type targetType, TypeConverterContext context = null)
        {
            if (context is null) return false;
            if (targetType.IsEnum && (
                context.CanConvert(sourceType, typeof(string)) ||
                context.CanConvert(sourceType, typeof(int))
            )) return true;
            if (sourceType.IsEnum && (
                context.CanConvert(targetType, typeof(string)) ||
                context.CanConvert(targetType, typeof(int))
            )) return true;
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
            if (targetType.IsEnum)
            {

                if (context.TryConvert(sourceType, typeof(string), in input, out object convertedString))
                {
                    if (convertedString is not string representation)
                    {
                        output = null;
                        return false;
                    }

                    if (Enum.TryParse(targetType, representation, out output))
                        return true;
                    else
                        return false;
                }
                else if (context.TryConvert(sourceType, typeof(int), in input, out object convertedInt))
                {
                    if (convertedInt is not int value)
                    {
                        output = null;
                        return false;
                    }

                    output = Enum.ToObject(targetType, value);
                    return true;
                }
            }
            else
            {
                output = Enum.GetName(targetType, input);
                return true;
            }

            // If we reach this point, we failed to convert.
            output = null;
            return false;
        }
    }
}