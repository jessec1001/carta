using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using CartaCore.Operations.Attributes;
using Namotion.Reflection;

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
        public override bool TryConvert(Type type, object value, out object result, TypeConverterContext context = null)
        {
            // Check if we are converting from a class or struct.
            if (IsDictionaryType(type))
            {
                IDictionary dictionary = value
                    .GetType()
                    .GetProperties(
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.IgnoreCase
                    )
                    .ToDictionary(
                        property => property.Name,
                        property => property.GetValue(value)
                    );
                result = dictionary;
                return true;
            }
            // Otherwise, we are converting from a dictionary.
            else
            {
                // Check if the value is a dictionary.
                if (value is not IDictionary dictionary)
                {
                    result = null;
                    return false;
                }

                // We create a new instance of the target type.
                result = Activator.CreateInstance(type);
                foreach (DictionaryEntry entry in dictionary)
                {
                    // We get the property of the target type.
                    PropertyInfo property = type.GetProperty(
                        entry.Key.ToString(),
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.IgnoreCase
                    );
                    if (property is null) continue;

                    // We use other converters to convert the value.
                    object convertedValue;
                    if (context is not null) context.TryConvert(property.PropertyType, entry.Value, out convertedValue);
                    else convertedValue = entry.Value;

                    // We set the value of the property.
                    property.SetValue(result, convertedValue);
                }
                foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase))
                {
                    // TODO: Generalize this so that it is the attribute that provides this extension.
                    OperationAuthenticationAttribute attribute = property.GetCustomAttribute<OperationAuthenticationAttribute>();
                    if (attribute is not null)
                    {
                        // Check for the prefix entry in the dictionary.
                        if (dictionary.Contains(attribute.Prefix))
                        {
                            object authField = dictionary[attribute.Prefix];
                            IDictionary authDict = authField as IDictionary;

                            // Look inside the authentication dictionary for the specified entry.
                            if (authDict is not null && authDict.Contains(attribute.Key))
                            {
                                // We need to take the authentication value and initialize the corresponding property
                                // of the target type from a constructor.
                                object authValue = authDict[attribute.Key];
                                object[] parameters = { authValue };
                                ConstructorInfo constructor = property.PropertyType.GetConstructor
                                (
                                    new Type[] { authValue.GetType() }
                                );
                                if (constructor is not null) property.SetValue(result, constructor.Invoke(parameters));
                            }
                        }

                        // This prevents any other value from being assigned to the authentication property.
                        continue;
                    }
                }
                return true;
            }
        }
    }
}