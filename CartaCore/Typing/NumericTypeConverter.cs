using System;

namespace CartaCore.Typing
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

            // We perform some special handling for booleans.
            if (input is bool boolean) return TryConvert(sourceType, targetType, boolean ? 1 : 0, out output, context);
            if (targetType.IsAssignableTo(typeof(bool)))
            {
                output = input.Equals(0);
                return true;
            }

            // Otherwise, let the built-in converter handle the conversion.
            output = Convert.ChangeType(input, targetType);
            return true;
        }
    }
}