using System;

namespace CartaCore.Typing.Conversion
{
    /// <summary>
    /// Converts values between compatible array types.
    /// </summary>
    public class ArrayTypeConverter : TypeConverter
    {
        /// <inheritdoc />
        public override bool CanConvert(Type sourceType, Type targetType, TypeConverterContext context = null)
        {
            // Types are only convertible if they are both of type array (of same rank)
            // and their element types are convertible.
            if (!sourceType.IsArray || !targetType.IsArray) return false;
            if (sourceType.GetArrayRank() != targetType.GetArrayRank()) return false;
            if (context is not null) return context.CanConvert(sourceType.GetElementType(), targetType.GetElementType());
            else return false;
        }
        /// <inheritdoc />
        public override bool TryConvert(Type type, object value, out object result, TypeConverterContext context = null)
        {
            // Create a new target array of the given element type and length.
            Type elementType = type.GetElementType();
            Array sourceArray = (Array)value;
            Array targetArray = Array.CreateInstance(elementType, sourceArray.Length);

            // Convert each element from the source array to the target array.
            for (int k = 0; k < sourceArray.Length; k++)
            {
                // Try to convert the source value to the target value.
                object sourceValue = sourceArray.GetValue(k);
                if (context is not null && context.TryConvert(elementType, sourceValue, out object targetValue))
                    targetArray.SetValue(targetValue, k);
                else
                {
                    // If the conversion failed, we return false.
                    result = null;
                    return false;
                }
            }

            // If the conversion succeeded, we return the target array.
            result = targetArray;
            return true;
        }
    }
}