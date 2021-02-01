using System;
using System.Collections.Generic;
using System.Linq;

namespace CartaCore.Utility
{
    public static class TypeExtensions
    {
        private static IDictionary<Type, string> Mappings;
        private static IDictionary<string, Type> InverseMappings;

        static TypeExtensions()
        {
            Mappings = new Dictionary<Type, string>();

            Mappings.Add(typeof(object), "object");
            Mappings.Add(typeof(string), "string");
            Mappings.Add(typeof(bool), "bool");
            Mappings.Add(typeof(byte), "byte");
            Mappings.Add(typeof(char), "char");
            Mappings.Add(typeof(decimal), "decimal");
            Mappings.Add(typeof(double), "double");
            Mappings.Add(typeof(short), "short");
            Mappings.Add(typeof(int), "int");
            Mappings.Add(typeof(long), "long");
            Mappings.Add(typeof(sbyte), "sbyte");
            Mappings.Add(typeof(float), "float");
            Mappings.Add(typeof(ushort), "ushort");
            Mappings.Add(typeof(uint), "uint");
            Mappings.Add(typeof(ulong), "ulong");
            Mappings.Add(typeof(void), "void");

            InverseMappings = Mappings.ToDictionary(
                pair => pair.Value,
                pair => pair.Key
            );
        }

        public static string ToFriendlyString(this Type type)
        {
            if (!(type is null) && Mappings.TryGetValue(type, out string result))
                return result;
            return type.Name;
        }
        public static Type ToFriendlyType(this string str)
        {
            if (!(str is null) && InverseMappings.TryGetValue(str, out Type result))
                return result;
            return typeof(string);
        }
    }
}