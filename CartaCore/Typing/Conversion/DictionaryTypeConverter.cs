using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace CartaCore.Typing.Conversion
{
    // TODO: Do we need a source type to make sure that the source type is the same as the input type?
    // TODO: Should we split converters of this form into two classes?
    /// <summary>
    /// Converts values between a dictionary type and a defined struct or class.
    /// </summary>
    public class DictionaryTypeConverter : TypeConverter
    {
        /// <summary>
        /// Checks if a type is a dictionary type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>
        /// <c>true</c> if the type is assignable to a dictionary with any generic types; otherwise <c>false</c>.
        /// </returns>
        protected static bool IsDictionaryType(Type type) => type.IsAssignableTo(typeof(IDictionary));
        /// <summary>
        /// Checks if a type is produced by a defined class.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns><c>true</c> if the type is that of a class; otherwise, <c>false</c>.</returns>
        protected static bool IsClassType(Type type) => type.IsClass;
        /// <summary>
        /// Checks if a type is produced by a defined struct.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns><c>true</c> if the type is that of a struct; otherwise, <c>false</c>.</returns>
        protected static bool IsStructType(Type type) => type.IsValueType && !type.IsEnum;

        /// <inheritdoc />
        public override bool CanConvert(Type sourceType, Type targetType, TypeConverterContext context = null)
        {
            // We check that there is one dictionary type and one class or struct type.
            if (IsDictionaryType(sourceType) && (IsClassType(targetType) || IsStructType(targetType))) return true;
            if (IsDictionaryType(targetType) && (IsClassType(sourceType) || IsStructType(sourceType))) return true;
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

            // Check if we are converting from a class or struct.
            if (IsDictionaryType(targetType))
            {
                object inputCopy = input;
                IDictionary dictionary = sourceType
                    .GetProperties(
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.IgnoreCase
                    )
                    .ToDictionary(
                        property => property.Name,
                        property => property.GetValue(inputCopy)
                    );
                output = dictionary;
                return true;
            }

            // Otherwise, we are converting from a dictionary.
            else
            {
                // Check if the value is a dictionary.
                if (input is not IDictionary dictionary)
                {
                    output = null;
                    return false;
                }

                // We create a new instance of the target type.
                output = Activator.CreateInstance(targetType);
                foreach (DictionaryEntry entry in dictionary)
                {
                    // We get the property of the target type.
                    PropertyInfo property = targetType.GetProperty(
                        entry.Key.ToString(),
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.IgnoreCase
                    );
                    if (property is null) continue;

                    // We use other converters to convert the value.
                    object convertedValue;
                    if (context is not null)
                    {
                        context.TryConvert(
                            entry.Value.GetType(),
                            property.PropertyType,
                            entry.Value,
                            out convertedValue
                        );
                    }
                    else convertedValue = entry.Value;

                    // We set the value of the property.
                    property.SetValue(output, convertedValue);
                }
                return true;
            }
        }
    }
}