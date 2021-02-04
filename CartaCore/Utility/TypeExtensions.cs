using System;
using System.Collections.Generic;
using System.Linq;

namespace CartaCore.Utility
{
    public static class TypeExtensions
    {
        private static IDictionary<Type, string> Primatives;
        private static IDictionary<string, Type> Names;

        static TypeExtensions()
        {
            Primatives = new Dictionary<Type, string>();

            Primatives.Add(typeof(object), "object");
            Primatives.Add(typeof(string), "string");
            Primatives.Add(typeof(bool), "boolean");
            Primatives.Add(typeof(byte), "byte");
            Primatives.Add(typeof(char), "char");
            Primatives.Add(typeof(decimal), "decimal");
            Primatives.Add(typeof(double), "double");
            Primatives.Add(typeof(short), "short");
            Primatives.Add(typeof(int), "integer");
            Primatives.Add(typeof(long), "long");
            Primatives.Add(typeof(sbyte), "sbyte");
            Primatives.Add(typeof(float), "float");
            Primatives.Add(typeof(ushort), "ushort");
            Primatives.Add(typeof(uint), "uint");
            Primatives.Add(typeof(ulong), "ulong");
            Primatives.Add(typeof(void), "void");

            Primatives.Add(typeof(DateTime), "date");
            Primatives.Add(typeof(Uri), "uri");

            Names = Primatives.ToDictionary(
                pair => pair.Value,
                pair => pair.Key
            );
        }

        public static string TypeSerialize(this Type type)
        {
            if (!(type is null) && Primatives.TryGetValue(type, out string result))
                return result;
            return type.Name;
        }
        public static Type TypeDeserialize(this string str)
        {
            if (!(str is null) && Names.TryGetValue(str, out Type result))
                return result;
            return typeof(string);
        }
    }
}