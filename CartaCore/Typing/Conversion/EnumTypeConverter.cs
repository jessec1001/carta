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
        public override bool TryConvert(Type type, object value, out object result, TypeConverterContext context = null)
        {
            if (value is string representation)
            {
                result = Enum.Parse(type, representation);
                return true;
            }
            else
            {
                result = Enum.GetName(type, value);
                return true;
            }
        }
    }
}