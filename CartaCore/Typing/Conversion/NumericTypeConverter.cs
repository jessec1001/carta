using System;

namespace CartaCore.Typing.Conversion
{
    /// <summary>
    /// Converts values between any primitive numeric types. 
    /// </summary>
    public class NumericTypeConverter : TypeConverter
    {
        /// <summary>
        /// Determines if a type one of the numeric primitive types or assignable to one.
        /// </summary>
        /// <param name="type">The type to check if numeric.</param>
        /// <returns><c>true</c> if type is numeric; otherwise, <c>false</c>.</returns>
        protected static bool IsNumericType(Type type)
        {
            // Notice that we include bool as a numeric type.
            if (type.IsAssignableTo(typeof(sbyte))) return true;
            if (type.IsAssignableTo(typeof(byte))) return true;
            if (type.IsAssignableTo(typeof(ushort))) return true;
            if (type.IsAssignableTo(typeof(short))) return true;
            if (type.IsAssignableTo(typeof(uint))) return true;
            if (type.IsAssignableTo(typeof(int))) return true;
            if (type.IsAssignableTo(typeof(ulong))) return true;
            if (type.IsAssignableTo(typeof(long))) return true;
            if (type.IsAssignableTo(typeof(float))) return true;
            if (type.IsAssignableTo(typeof(double))) return true;
            if (type.IsAssignableTo(typeof(decimal))) return true;
            if (type.IsAssignableTo(typeof(bool))) return true;
            return false;
        }

        /// <inheritdoc />
        public override bool CanConvert(Type sourceType, Type targetType, TypeConverterContext context = null)
        {
            return IsNumericType(sourceType) && IsNumericType(targetType);
        }
        /// <inheritdoc />
        public override bool TryConvert(Type type, object value, out object result, TypeConverterContext context = null)
        {
            // We perform some special handling for booleans.
            if (value is bool boolean) return TryConvert(type, boolean ? 1 : 0, out result, context);
            if (type.IsAssignableTo(typeof(bool)))
            {
                result = value.Equals(0);
                return true;
            }

            // Otherwise, let the built-in converter handle the conversion.
            result = Convert.ChangeType(value, type);
            return true;
        }
    }
}