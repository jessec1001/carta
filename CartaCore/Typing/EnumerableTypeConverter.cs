using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CartaCore.Typing
{
    /// <summary>
    /// Converts from one enumerable type to another.
    /// </summary>
    public class EnumerableTypeConverter : TypeConverter
    {
        /// <summary>
        /// Determines whether the specified type is an enumerable type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="elementType">The element type if the type is enumerable.</param>
        /// <returns>Whether the type is enumerable or not.</returns>
        private static bool ImplementsEnumerable(Type type, out Type elementType)
        {
            // Check if the type itself is enumerable.
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                elementType = type.GetGenericArguments().First();
                return true;
            }
            // Check if the type derives from an enumerable type.
            else
            {

                foreach (Type interfaceType in type.GetInterfaces())
                {
                    if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    {
                        elementType = interfaceType.GetGenericArguments().First();
                        return true;
                    }
                }
            }
            elementType = default;
            return false;
        }
        /// <inheritdoc />
        public static bool CanConvert(
            Type sourceType,
            Type targetType,
            out Type sourceElementType,
            out Type targetElementType)
        {
            // Types are only convertible if they are both enumerable.
            if (!ImplementsEnumerable(sourceType, out sourceElementType))
            {
                sourceElementType = targetElementType = default;
                return false;
            }
            if (!ImplementsEnumerable(targetType, out targetElementType))
            {
                sourceElementType = targetElementType = default;
                return false;
            }

            // Check if the element types are compatible.
            return true;
        }
        /// <inheritdoc />
        public override bool CanConvert(
            Type sourceType,
            Type targetType,
            TypeConverterContext context = null)
        {
            return CanConvert(sourceType, targetType, out _, out _);
        }
        /// <inheritdoc />
        public override bool TryConvert(
            Type sourceType,
            Type targetType,
            in object input,
            out object output,
            TypeConverterContext context = null)
        {
            // Check that the source and target types are compatible.
            if (!CanConvert(sourceType, targetType, out Type sourceElementType, out Type targetElementType))
            {
                output = null;
                return false;
            }

            // Create a new target enumerable of the given element type.
            try
            {
                MethodInfo genericEnumerate = typeof(EnumerableTypeConverter).GetMethod(
                    nameof(Enumerate),
                    BindingFlags.NonPublic | BindingFlags.Static
                );
                MethodInfo concreteEnumerate = genericEnumerate.MakeGenericMethod(targetElementType);

                output = concreteEnumerate.Invoke(null, new object[]
                {
                    targetElementType,
                    input as IEnumerable,
                    context
                });
                return true;
            }
            catch
            {
                output = null;
                return false;
            }
        }

        /// <summary>
        /// Enumerates over an enumeration of values and converts each value to the given type. 
        /// </summary>
        /// <param name="targetElementType">The target element type.</param>
        /// <param name="values">The enumeration of values.</param>
        /// <param name="context">The context for type conversion.</param>
        /// <returns>The converted enumeration.</returns>
        private static IEnumerable<T> Enumerate<T>(
            Type targetElementType,
            IEnumerable values,
            TypeConverterContext context = null)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            foreach (object value in values)
            {
                Type sourceElementType = value?.GetType() ?? typeof(object);
                if (!context.TryConvert(sourceElementType, targetElementType, value, out object converted))
                    throw new InvalidCastException();
                yield return (T)converted;
            }
        }
    }
}