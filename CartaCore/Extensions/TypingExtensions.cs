using System;
using System.Collections.Generic;
using System.Reflection;
using CartaCore.Operations.Attributes;
using CartaCore.Typing;

namespace CartaCore.Extensions.Typing
{
    /// <summary>
    /// Provides utility methods for working with typing and type conversions.
    /// </summary>
    public static class TypingExtensions
    {
        /// <summary>
        /// Converts a dictionary into a typed value.
        /// </summary>
        /// <param name="fields">The dictionary of fields.</param>
        /// <param name="type">The type of value.</param>
        /// <param name="context">The type conversion context. Optional.</param>
        /// <returns>The converted value.</returns>
        public static object AsTyped(
            this Dictionary<string, object> fields,
            Type type,
            TypeConverterContext context = null)
        {
            TypeConverter converter = (TypeConverter)context ?? new DictionaryTypeConverter();
            if (converter.TryConvert(fields.GetType(), type, fields, out object result)) return result;
            throw new InvalidOperationException($"Could not convert dictionary to type {type.Name}.");
        }
        /// <summary>
        /// Converts a dictionary into a typed value.
        /// </summary>
        /// <param name="fields">The dictionary of fields.</param>
        /// <param name="context">The type conversion context. Optional.</param>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <returns>The converted value.</returns>
        public static T AsTyped<T>(this Dictionary<string, object> fields, TypeConverterContext context = null)
        {
            return (T)AsTyped(fields, typeof(T), context);
        }
        /// <summary>
        /// Converts a value into a field dictionary.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="type">The type of value.</param>
        /// <param name="context">The type conversion context. Optional.</param>
        /// <returns>The converted dictionary.</returns>
        public static Dictionary<string, object> AsDictionary(
            this object value,
            Type type,
            TypeConverterContext context = null)
        {
            TypeConverter converter = (TypeConverter)context ?? new DictionaryTypeConverter();
            if (converter.TryConvert(type, typeof(Dictionary<string, object>), value, out object result))
                return result as Dictionary<string, object>;
            throw new InvalidOperationException($"Could not convert value to dictionary.");
        }
        /// <summary>
        /// Converts a value into a field dictionary.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="context">The type conversion context. Optional.</param>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <returns>The converted dictionary.</returns>
        public static Dictionary<string, object> AsDictionary<T>(this T value, TypeConverterContext context = null)
        {
            return AsDictionary(value, typeof(T), context);
        }

        /// <summary>
        /// Modifies an existing type converter by applying a collection of attributes.
        /// Notice that this method is safe to rely on from external code because we can construct attributes without
        /// needing a reference to member information.
        /// </summary>
        /// <param name="context">The type converter context to modify.</param>
        /// <param name="attributes">The attributes to apply.</param>
        /// <returns>A modified type converter context.</returns>
        public static TypeConverterContext ApplyAttributes(
            this TypeConverterContext context,
            IEnumerable<Attribute> attributes
        )
        {
            // Default for a null context.
            if (context == null) context = new TypeConverterContext();

            // Construct a new context with any type converter attributes applied to the stack.
            List<TypeConverter> converters = new(context.TypeConverters);
            foreach (Attribute attribute in attributes)
            {
                // TODO: Reimplement.
                // if (attribute is ITypeConverterAttribute typeConverterAttribute)
                // typeConverterAttribute.ApplyConverter(converters);
            }
            return new TypeConverterContext(converters.ToArray());
        }

        /// <summary>
        /// Infers the type of a generic type by specifying the type of a property within it.
        /// </summary>
        /// <param name="baseType">The base type.</param>
        /// <param name="propertyType">The property type.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The inferred type.</returns>
        public static Type InferType(this Type baseType, Type propertyType, string propertyName)
        {
            // Shortcut check if the base type is generic.
            if (!baseType.IsGenericType) return baseType;
            Type genericType = baseType.GetGenericTypeDefinition();

            // Get the property that we are trying to infer.
            PropertyInfo property = genericType.GetProperty(
                propertyName,
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.IgnoreCase
            );
            if (property is null) throw new Exception("Property not found.");

            // Check if the property type is a generic type parameter.
            Type existingType = property.PropertyType;
            if (existingType.IsGenericTypeParameter)
            {
                // Find the corresponding type parameter in the base type.
                Type[] genericArguments = baseType.GetGenericArguments();
                genericArguments[existingType.GenericParameterPosition] = propertyType;

                // Create a new generic type with the inferred type parameter.
                Type inferredType = baseType.GetGenericTypeDefinition().MakeGenericType(genericArguments);
                return inferredType;
            }

            return baseType;
        }

        /// <summary>
        /// Checks if a type is an instantiation of a particular generic type.
        /// If so, yields the type arguments.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="genericDef">The generic type definition to match against.</param>
        /// <param name="genericArgs">The generic arguments used to instantiate the type.</param>
        /// <returns><c>true</c> if the type is an instance of the generic type; <c>false</c> otherwise.</returns>
        public static bool ImplementsGenericInterface(this Type type, Type genericDef, out Type[] genericArgs)
        {
            // Initialize the generic arguments.
            genericArgs = null;

            // Check interfaces until one with matching criteria is found.
            foreach (Type interfaceType in type.GetInterfaces())
            {
                // Check if the interface is a generic type.
                if (!interfaceType.IsGenericType) continue;

                // Check if the generic type definition matches.
                Type interfaceDef = interfaceType.GetGenericTypeDefinition();
                if (interfaceDef != genericDef) continue;

                // Get the generic arguments.
                genericArgs = interfaceType.GetGenericArguments();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Executes a generic method with specified arguments.
        /// The method is assumed to be static.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="typeArgs">The type arguments.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>The result of calling the function.</returns>
        public static object InvokeGenericFunc(this MethodInfo method, Type[] typeArgs, params object[] args)
        {
            // Get the method info.
            MethodInfo genericMethod = method.GetGenericMethodDefinition();
            MethodInfo genericMethodInfo = genericMethod.MakeGenericMethod(typeArgs);

            // Invoke the method.
            return genericMethodInfo.Invoke(null, args);
        }
    }
}