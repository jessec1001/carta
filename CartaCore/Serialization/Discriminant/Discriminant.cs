using System;
using System.Collections.Generic;
using System.Reflection;

namespace CartaCore.Serialization
{
    public static class Discriminant
    {
        private static Dictionary<Type, Dictionary<string, Type>> Derivatives;
        private static Dictionary<Type, Dictionary<string, (Type, Func<object>)>> Aliases;

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

        public static bool TryGetType(Type baseType, string discriminant, out Type derivedType)
        {
            derivedType = null;
            if (!Derivatives.TryGetValue(baseType, out Dictionary<string, Type> baseDerivatives) ||
                !Aliases.TryGetValue(baseType, out Dictionary<string, (Type, Func<object>)> baseAliases))
                return false;

            if (baseDerivatives.TryGetValue(discriminant, out Type type))
            {
                derivedType = type;
                return true;
            }
            if (baseAliases.TryGetValue(discriminant, out (Type type, Func<object> func) alias))
            {
                derivedType = alias.type;
                return true;
            }
            return false;
        }
        public static bool TryGetType<T>(string discriminant, out Type derivedType)
        {
            bool success = TryGetType(typeof(T), discriminant, out Type type);
            derivedType = type;
            return success;
        }

        public static bool TryGetValue(Type baseType, string discriminant, out object value)
        {
            value = null;
            if (!TryGetType(baseType, discriminant, out Type derivedType))
                return false;

            if (Aliases[baseType].TryGetValue(discriminant, out (Type type, Func<object> func) alias))
            {
                value = alias.func();
                return true;
            }

            value = derivedType
                .GetConstructor(Type.EmptyTypes)
                .Invoke(new object[] { });
            return true;
        }
        public static bool TryGetValue<T>(string discriminant, out T value)
        {
            bool success = TryGetValue(typeof(T), discriminant, out object objValue);
            if (success)
                value = (T)objValue;
            else
                value = default(T);
            return success;
        }
    }
}