using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using CartaCore.Extensions.Typing;

namespace CartaCore.Typing
{
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
                foreach (PropertyInfo property in targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    // Check if the property is in the dictionary.
                    DictionaryEntry entry = default;
                    bool entryExists = false;
                    foreach (DictionaryEntry dictionaryEntry in dictionary)
                    {
                        string key = dictionaryEntry.Key.ToString();
                        if (key.Equals(property.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            entry = dictionaryEntry;
                            entryExists = true;
                            break;
                        }
                    }

                    // We attempt to convert the property.
                    if (entryExists)
                    {
                        object convertedValue;
                        if (context is not null)
                        {
                            // By default, use the context to convert the value.
                            TypeConverter converter = context;

                            // Create a new type converter context based on the property attributes.
                            TypeConverterContext subcontext = context.ApplyAttributes(property.GetCustomAttributes());

                            // Convert the value.
                            subcontext.TryConvert(
                                entry.Value?.GetType() ?? typeof(object),
                                property.PropertyType,
                                entry.Value,
                                out convertedValue
                            );
                        }
                        else convertedValue = entry.Value;

                        // We set the value of the property.
                        property.SetValue(output, convertedValue);
                    }
                }
                return true;
            }
        }
    }
}