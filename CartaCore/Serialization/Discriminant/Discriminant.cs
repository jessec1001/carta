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
        private static Dictionary<Type, Dictionary<string, DiscriminantType>> Derivatives;
        /// <summary>
        /// Contains mappings between a base type and its alias types and functions with their discriminants as keys.
        /// </summary>
        private static Dictionary<Type, Dictionary<string, DiscriminantAlias>> Aliases;

        /// <summary>
        /// Sets up the mappings between types and discriminants.
        /// </summary>
        static Discriminant()
        {
            // We perform some start-up initialization to find all the base and derived types with discriminants.
            Derivatives = new Dictionary<Type, Dictionary<string, DiscriminantType>>();
            Aliases = new Dictionary<Type, Dictionary<string, DiscriminantAlias>>();

            Type[] assemblyTypes = Assembly.GetCallingAssembly().GetTypes();
            foreach (Type baseType in assemblyTypes)
            {
                // Base types should have the base attribute.
                DiscriminantBaseAttribute baseAttr = baseType.GetCustomAttribute<DiscriminantBaseAttribute>();
                DiscriminantDerivedAttribute baseDerivedAttr = baseType.GetCustomAttribute<DiscriminantDerivedAttribute>();
                if (baseAttr is null) continue;
                if (baseDerivedAttr is not null) continue;

                // Add the base class lookup.
                Derivatives.Add(baseType, new Dictionary<string, DiscriminantType>());
                Aliases.Add(baseType, new Dictionary<string, DiscriminantAlias>());

                foreach (Type derivedType in assemblyTypes)
                {
                    // Derived types should have the derived attribute and be an actual derived type.
                    // Derived types might also have a semantics attribute that defines its display name and grouping.
                    DiscriminantDerivedAttribute derivedAttr = derivedType.GetCustomAttribute<DiscriminantDerivedAttribute>();
                    DiscriminantSemanticsAttribute semanticsAttr = derivedType.GetCustomAttribute<DiscriminantSemanticsAttribute>();
                    if (derivedAttr is null) continue;
                    if (!baseType.IsAssignableFrom(derivedType)) continue;

                    // Check for correct construction method.
                    if (derivedType.GetConstructor(Type.EmptyTypes) is null)
                        throw new NotSupportedException($"Derived discriminant class {derivedType} must have a parameterless constructor.");

                    // Add the derived class lookup.
                    DiscriminantType discriminantType = new DiscriminantType
                    {
                        Type = derivedType,
                        Discriminant = derivedAttr.Discriminant,
                        Hidden = semanticsAttr is null ? true : false,
                        Name = semanticsAttr?.Name,
                        Group = semanticsAttr?.Group,
                    };
                    Derivatives[baseType].Add(derivedAttr.Discriminant, discriminantType);

                    // Add the derived class lookup aliases.
                    foreach (MethodInfo method in derivedType.GetMethods(BindingFlags.Static | BindingFlags.Public))
                    {
                        foreach (DiscriminantAliasAttribute aliasAttr in method.GetCustomAttributes<DiscriminantAliasAttribute>())
                        {
                            // Check if there is also a semantics attribute.
                            DiscriminantSemanticsAttribute methodSemanticsAttr = method.GetCustomAttribute<DiscriminantSemanticsAttribute>();

                            // Create the method.
                            if (!method.ReturnType.IsAssignableTo(derivedType))
                                throw new NotSupportedException($"Derived discriminant class {derivedType} has alias {aliasAttr.AliasName} that does not return a value assignable to its containing type.");
                            if (method.GetParameters().Length != 0)
                                throw new NotSupportedException($"Derived discriminant class {derivedType} has alias {aliasAttr.AliasName} that is not parameterless.");
                            Func<object> func = (Func<object>)Delegate.CreateDelegate(typeof(Func<object>), method);

                            DiscriminantAlias discriminantAlias = new DiscriminantAlias
                            {
                                Type = derivedType,
                                Discriminant = aliasAttr.AliasName,
                                Generator = func,
                                Hidden = methodSemanticsAttr is null ? true : false,
                                Name = methodSemanticsAttr?.Name,
                                Group = methodSemanticsAttr?.Group
                            };
                            Aliases[baseType].Add(aliasAttr.AliasName, discriminantAlias);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the derived types with the specified base type. Returns a boolean that represents whether the lookup
        /// was successful.
        /// </summary>
        /// <param name="baseType">The base discriminant type.</param>
        /// <param name="types">The enumerable of derived types found by lookup or <c>null</c> if none exists.</param>
        /// <returns><c>true</c> if the lookup returned a valid enumerable of types; otherwise, <c>false</c>.</returns>
        public static bool TryGetTypes(Type baseType, out IEnumerable<DiscriminantType> types)
        {
            // Check for a base type mapping and return its corresponding list of derived types.
            if (Derivatives.TryGetValue(baseType, out Dictionary<string, DiscriminantType> derivedTypes))
            {
                types = derivedTypes.Values;
                return true;
            }
            else
            {
                types = null;
                return false;
            }
        }
        /// <summary>
        /// Gets the derived types with the specified base type. Returns a boolean that represents whether the lookup
        /// was successful.
        /// </summary>
        /// <param name="types">The enumerable of derived types found by lookup or <c>null</c> if none exists.</param>
        /// <returns><c>true</c> if the lookup returned a valid enumerable of types; otherwise, <c>false</c>.</returns>
        /// <typeparam name="T">The base discriminant type.</typeparam>
        public static bool TryGetTypes<T>(out IEnumerable<DiscriminantType> types)
        {
            // Use the non-generic version of this function to get the types.
            bool success = TryGetTypes(typeof(T), out IEnumerable<DiscriminantType> derivedTypes);
            types = derivedTypes;
            return success;
        }

        /// <summary>
        /// Gets the derived type with the specified base type and discriminant. Returns a boolean that represents
        /// whether the lookup was successful.
        /// </summary>
        /// <param name="baseType">The base discriminant type.</param>
        /// <param name="discriminant">The discriminant of the derived type.</param>
        /// <param name="derivedType">The derived type found by lookup or <c>null</c> if none exists.</param>
        /// <returns><c>true</c> if the lookup returned a valid type; otherwise, <c>false</c>.</returns>
        public static bool TryGetType(Type baseType, string discriminant, out DiscriminantType derivedType)
        {
            // If the base type cannot be found in both the derivatives and alias dictionaries, then, there will be no
            // corresponding derived types.
            derivedType = null;
            if (!Derivatives.TryGetValue(baseType, out Dictionary<string, DiscriminantType> baseDerivatives) ||
                !Aliases.TryGetValue(baseType, out Dictionary<string, DiscriminantAlias> baseAliases))
                return false;

            // Check for a normal discriminant type.
            if (baseDerivatives.TryGetValue(discriminant, out DiscriminantType type))
            {
                derivedType = type;
                return true;
            }
            // Check for an aliased discriminant type.
            if (baseAliases.TryGetValue(discriminant, out DiscriminantAlias alias))
            {
                derivedType = alias;
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
        public static bool TryGetType<T>(string discriminant, out DiscriminantType derivedType)
        {
            // Use the non-generic version of this function to perform the search.
            bool success = TryGetType(typeof(T), discriminant, out DiscriminantType type);
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
            if (!TryGetType(baseType, discriminant, out DiscriminantType derivedType))
                return false;

            // If the discriminant is actually an alias, we need to use the alias function to construct a parameterized
            // instance of the type.
            if (Aliases[baseType].TryGetValue(discriminant, out DiscriminantAlias alias))
            {
                value = alias.Generator();
                return true;
            }

            // If the discriminant is not an alias, we use the parameterless constructor of the type.
            value = derivedType.Type
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