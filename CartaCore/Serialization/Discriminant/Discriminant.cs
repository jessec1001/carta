using System;
using System.Collections.Generic;
using System.Reflection;

namespace CartaCore.Serialization
{
    /// <summary>
    /// Helper utilities for working with discriminant types. Compiles a mapping of discriminants and aliases to their
    /// corresponding types and default values.
    /// </summary>
    public static class Discriminant
    {
        /// <summary>
        /// Contains mappings between a base type and its derived types with their discriminants as keys. 
        /// </summary>
        private static Dictionary<Type, Dictionary<string, Type>> Derivatives;
        /// <summary>
        /// Contains mappings between a base type and its alias types and functions with their discriminants as keys.
        /// </summary>
        private static Dictionary<Type, Dictionary<string, (Type, Func<object>)>> Aliases;

        /// <summary>
        /// Sets up the mappings between types and discriminants.
        /// </summary>
        static Discriminant()
        {
            // We perform some start-up initialization to find all the base and derived types with discriminants.
            Derivatives = new Dictionary<Type, Dictionary<string, Type>>();
            Aliases = new Dictionary<Type, Dictionary<string, (Type, Func<object>)>>();

            Type[] assemblyTypes = Assembly.GetCallingAssembly().GetTypes();
            foreach (Type baseType in assemblyTypes)
            {
                // Base types should have the base attribute.
                DiscriminantBaseAttribute baseAttr = baseType.GetCustomAttribute<DiscriminantBaseAttribute>();
                DiscriminantDerivedAttribute baseDerivedAttr = baseType.GetCustomAttribute<DiscriminantDerivedAttribute>();
                if (baseAttr is null) continue;
                if (baseDerivedAttr is not null) continue;

                // Add the base class lookup.
                Derivatives.Add(baseType, new Dictionary<string, Type>());
                Aliases.Add(baseType, new Dictionary<string, (Type, Func<object>)>());

                foreach (Type derivedType in assemblyTypes)
                {
                    // Derived types should have the derived attribute and be an actual derived type.
                    DiscriminantDerivedAttribute derivedAttr = derivedType.GetCustomAttribute<DiscriminantDerivedAttribute>();
                    if (derivedAttr is null) continue;
                    if (!baseType.IsAssignableFrom(derivedType)) continue;

                    // Check for correct construction method.
                    if (derivedType.GetConstructor(Type.EmptyTypes) is null)
                        throw new NotSupportedException($"Derived discriminant class {derivedType} must have a parameterless constructor.");

                    // Add the derived class lookup.
                    Derivatives[baseType].Add(derivedAttr.Discriminant, derivedType);

                    // Add the derived class lookup aliases.
                    foreach (MethodInfo method in derivedType.GetMethods(BindingFlags.Static | BindingFlags.Public))
                    {
                        foreach (DiscriminantAliasAttribute aliasAttr in method.GetCustomAttributes<DiscriminantAliasAttribute>())
                        {
                            // Create the method.
                            if (!method.ReturnType.IsAssignableTo(derivedType))
                                throw new NotSupportedException($"Derived discriminant class {derivedType} has alias {aliasAttr.AliasName} that does not return a value assignable to its containing type.");
                            if (method.GetParameters().Length != 0)
                                throw new NotSupportedException($"Derived discriminant class {derivedType} has alias {aliasAttr.AliasName} that is not parameterless.");
                            Func<object> func = (Func<object>)Delegate.CreateDelegate(typeof(Func<object>), method);

                            Aliases[baseType].Add(aliasAttr.AliasName, (derivedType, func));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the derived type with the specified base type and discriminant. Returns a boolean that represents
        /// whether the lookup was successful.
        /// </summary>
        /// <param name="baseType">The base discriminant type.</param>
        /// <param name="discriminant">The discriminant of the derived type.</param>
        /// <param name="derivedType">The derived type found by lookup or <c>null</c> if none exists.</param>
        /// <returns><c>true</c> if the lookup returned a valid type; otherwise, <c>false</c>.</returns>
        public static bool TryGetType(Type baseType, string discriminant, out Type derivedType)
        {
            // If the base type cannot be found in both the derivatives and alias dictionaries, then, there will be no
            // corresponding derived types.
            derivedType = null;
            if (!Derivatives.TryGetValue(baseType, out Dictionary<string, Type> baseDerivatives) ||
                !Aliases.TryGetValue(baseType, out Dictionary<string, (Type, Func<object>)> baseAliases))
                return false;

            // Check for a normal discriminant type.
            if (baseDerivatives.TryGetValue(discriminant, out Type type))
            {
                derivedType = type;
                return true;
            }
            // Check for an aliased discriminant type.
            if (baseAliases.TryGetValue(discriminant, out (Type type, Func<object> func) alias))
            {
                derivedType = alias.type;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Gets the derived type with the specified base type and discriminant. Returns a boolean that represents
        /// whether the lookup was successful.
        /// </summary>
        /// <param name="discriminant">The discriminant of the derived type.</param>
        /// <param name="derivedType">The derived type found by lookup or <c>null</c> if none exists.</param>
        /// <returns><c>true</c> if the lookup returned a valid type; otherwise, <c>false</c>.</returns>
        /// <typeparam name="T">The base discriminant type.</typeparam>
        public static bool TryGetType<T>(string discriminant, out Type derivedType)
        {
            // Use the non-generic version of this function to perform the search.
            bool success = TryGetType(typeof(T), discriminant, out Type type);
            derivedType = type;
            return success;
        }

        /// <summary>
        /// Gets the default value of a derived type with specified base type and discriminant. Returns a boolean that
        /// represents whether the lookup was successful. 
        /// </summary>
        /// <param name="baseType">The base discriminant type.</param>
        /// <param name="discriminant">The discriminant of the derived type.</param>
        /// <param name="value">The default value found by lookup or <c>null</c> if none exists.</param>
        /// <returns><c>true</c> if the lookup returned a valid value; otherwise <c>false</c>.</returns>
        public static bool TryGetValue(Type baseType, string discriminant, out object value)
        {
            // We need the discriminant type to perform the initialization of a value.
            value = null;
            if (!TryGetType(baseType, discriminant, out Type derivedType))
                return false;

            // If the discriminant is actually an alias, we need to use the alias function to construct a parameterized
            // instance of the type.
            if (Aliases[baseType].TryGetValue(discriminant, out (Type type, Func<object> func) alias))
            {
                value = alias.func();
                return true;
            }

            // If the discriminant is not an alias, we use the parameterless constructor of the type.
            value = derivedType
                .GetConstructor(Type.EmptyTypes)
                .Invoke(new object[] { });
            return true;
        }
        /// <summary>
        /// Gets the default value of a derived type with specified base type and discriminant. Returns a boolean that
        /// represents whether the lookup was successful. 
        /// </summary>
        /// <param name="discriminant">The discriminant of the derived type.</param>
        /// <param name="value">The default value found by lookup or <c>default(T)</c> if none exists.</param>
        /// <returns><c>true</c> if the lookup returned a valid value; otherwise <c>false</c>.</returns>
        /// <typeparam name="T">The base discriminant type.</typeparam>
        public static bool TryGetValue<T>(string discriminant, out T value)
        {
            // Use the non-generic version of this function to perform the search.
            bool success = TryGetValue(typeof(T), discriminant, out object objValue);
            if (success)
                value = (T)objValue;
            else
                value = default(T);
            return success;
        }
    }
}