using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace CartaCore.Extensions.Documentation
{
    /// <summary>
    /// A set of extensions used to retrieve XML documentation for the source code.
    /// </summary>
    public static class DocumentationExtensions
    {
        /// <summary>
        /// Stores loaded assemblies after they have been used.
        /// </summary>
        private static readonly Dictionary<Assembly, XmlDocument> LoadedAssemblies = new();
        /// <summary>
        /// Stores loaded members after they have been used.
        /// </summary>
        private static readonly Dictionary<string, (Assembly assembly, XmlNode node)> LoadedMembers = new();

        /// <summary>
        /// Gets the default path to the XML documentation of an assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>The XML documentation path.</returns>
        public static string GetDocumentationPath(this Assembly assembly)
        {
            string assemblyPath = assembly.Location;
            string assemblyDirectory = Path.GetDirectoryName(assemblyPath);
            string documentationPath = Path.Combine(assemblyDirectory, $"{assembly.GetName().Name}.xml");
            return documentationPath;
        }
        /// <summary>
        /// Loads the XML documentation of an assembly with an optionally specified path.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="path">
        /// The optional path to the XML documentation. If not specified, the default path is assumed.
        /// </param>
        /// <returns>The XML document representing the documentation.</returns>
        public static XmlDocument LoadDocumentation(this Assembly assembly, string path = null)
        {
            // Check if the assembly has already been loaded.
            if (LoadedAssemblies.TryGetValue(assembly, out XmlDocument xmlDocument))
                return xmlDocument;

            // Use default path if necessary.
            path ??= assembly.GetDocumentationPath();

            // Load the XML documentation file.
            xmlDocument = new();
            xmlDocument.PreserveWhitespace = false;
            xmlDocument.Load(path);

            // Store the loaded XML document.
            LoadedAssemblies.Add(assembly, xmlDocument);

            // Store each of the members.
            foreach (XmlNode node in xmlDocument.SelectNodes("//member"))
                LoadedMembers.Add(node.Attributes["name"].Value, (assembly, node));

            return xmlDocument;
        }
        /// <summary>
        /// Unloads the XML documentation of an assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        public static void UnloadDocumentation(this Assembly assembly)
        {
            // Unload the XML documentation file.
            LoadedAssemblies.Remove(assembly);

            // Unload all associated members in the assembly.
            List<string> removeableKeys = LoadedMembers
                .Where(pair => pair.Value.assembly == assembly)
                .Select(pair => pair.Key)
                .ToList();
            foreach (string key in removeableKeys)
                LoadedMembers.Remove(key);
        }

        /// <summary>
        /// Formats a key to match the formatting used to generate the XML documentation.
        /// </summary>
        /// <param name="fullName">The full name of the declared/declaring type.</param>
        /// <param name="memberName">The name of the type member.</param>
        /// <returns>The formatted key.</returns>
        private static string DocumentationKeyFormatter(string fullName, string memberName)
        {
            string key = Regex.Replace(fullName, @"\[.*\]", string.Empty).Replace('+', '.');
            if (memberName is not null) key += $".{memberName}";
            return key;
        }
        /// <summary>
        /// Formats a type to match the formatting used to generate the XML documentation.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The formatted type.</returns>
        private static string DocumentationTypeFormatter(Type type)
        {
            if (type.IsGenericType)
            {
                string genericArgumentNames = string.Join(
                    ',', type
                        .GetGenericArguments()
                        .Select(t => DocumentationTypeFormatter(t))
                );
                string genericTypeName = type.FullName[..type.FullName.IndexOf('`')];
                return $"{genericTypeName}{{{genericArgumentNames}}}";
            }
            else return type.FullName;
        }

        /// <summary>
        /// Gets the XML key for a specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>They key within the XML member documentation.</returns>
        private static string GetKey(Type type) => $"T:{DocumentationKeyFormatter(type.FullName, null)}";
        /// <summary>
        /// Gets the XML key for a specified method.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>The key within the XML member documentation.</returns>
        private static string GetKey(MethodInfo method)
        {
            string paramsKey = string.Join(',', method
                .GetParameters()
                .Select(param =>
                    DocumentationKeyFormatter(DocumentationTypeFormatter(param.ParameterType), null)
                )
            );
            if (string.IsNullOrEmpty(paramsKey))
                return $"M:{DocumentationKeyFormatter(method.DeclaringType.FullName, method.Name)}";
            else
                return $"M:{DocumentationKeyFormatter(method.DeclaringType.FullName, method.Name)}({paramsKey})";
        }

        /// <summary>
        /// Gets a node for an assembly under a specified member name.
        /// </summary>
        /// <param name="assembly">The assembly that a member belongs to.</param>
        /// <param name="memberName">The name of the member to obtain the XML node for.</param>
        /// <returns>The XML node containing the documentation about the specified member.</returns>
        private static XmlNode GetNode(Assembly assembly, string memberName)
        {
            // Load the XML node for the specified member.
            // Will try to load from a cache if possible.
            if (LoadedMembers.TryGetValue(memberName, out (Assembly assembly, XmlNode node) cachedNode))
                return cachedNode.node;
            else
            {
                // Load the XML document for the specified assembly.
                // Will try to load from a cache if possible.
                XmlDocument xmlDocument;
                if (LoadedAssemblies.TryGetValue(assembly, out XmlDocument cachedDocument))
                    xmlDocument = cachedDocument;
                else
                    xmlDocument = assembly.LoadDocumentation();

                // Get the specified node by its member name.
                return xmlDocument.SelectSingleNode($"//member[@name='{memberName}']");
            }
        }

        /// <summary>
        /// Gets the documentation for a type as a specified type of object.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <typeparam name="T">The type of documentation.</typeparam>
        /// <returns>The documentation object.</returns>
        public static T GetDocumentation<T>(this Type type)
        {
            // Get the member name of the type.
            Assembly memberAssembly = type.Assembly;
            string memberName = GetKey(type);

            // Load the node containing the type member.
            XmlNode xmlNode = GetNode(memberAssembly, memberName);
            if (xmlNode is null)
                throw new NullReferenceException($"Failed to find documentation for type '{type.Name}'.");

            // Deserialize the documentation into a specified type.
            XmlSerializer xmlSerializer = new(typeof(T));
            using XmlNodeReader reader = new(xmlNode);
            return (T)xmlSerializer.Deserialize(reader);
        }
        /// <summary>
        /// Get the documentation for a set of types as a specified type of object.
        /// </summary>
        /// <param name="types">The types.</param>
        /// <typeparam name="T">The type of documentation.</typeparam>
        /// <returns>The corresponding list of documentation objects.</returns>
        public static T[] GetDocumentation<T>(this Type[] types)
        {
            // Store values of types as they are loaded.
            List<T> memberValues = new(types.Length);

            // Load the nodes containing the type members and deserialize them.
            foreach (Type type in types)
            {
                // Get the member name of the types.
                Assembly memberAssembly = type.Assembly;
                string memberName = GetKey(type);

                // Load the particular node containing the type.
                XmlNode xmlNode = GetNode(memberAssembly, memberName);
                if (xmlNode is null)
                    throw new NullReferenceException($"Failed to find documentation for type '{type.Name}'.");

                // Deserialize the documentation into a specified type.
                XmlSerializer xmlSerializer = new(typeof(T));
                using XmlNodeReader reader = new(xmlNode);
                memberValues.Add((T)xmlSerializer.Deserialize(reader));
            }

            return memberValues.ToArray();
        }
        /// <summary>
        /// Gets the documentation for a method as a specified type of object.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <typeparam name="T">The type of documentation.</typeparam>
        /// <returns>The documentation object.</returns>
        public static T GetDocumentation<T>(this MethodInfo method)
        {
            // Get the member name of the method.
            Assembly memberAssembly = method.DeclaringType.Assembly;
            string memberName = GetKey(method);

            // Load the node containing the method member.
            XmlNode xmlNode = GetNode(memberAssembly, memberName);
            if (xmlNode is null)
                throw new NullReferenceException($"Failed to find documentation for method '{method.Name}'.");

            // Deserialize the documentation into a specified type.
            XmlSerializer xmlSerializer = new(typeof(T));
            using XmlNodeReader reader = new(xmlNode);
            return (T)xmlSerializer.Deserialize(reader);
        }
        /// <summary>
        /// Gets the documentation for a parameter of a method as a specified type of object.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <typeparam name="T">The type of documentation.</typeparam>
        /// <returns>The documentation object.</returns>
        public static T GetDocumentation<T>(this ParameterInfo parameter)
        {
            // Get the member name of the parameter.
            MethodInfo method = parameter.Member as MethodInfo;
            Assembly memberAssembly = method.DeclaringType.Assembly;
            string memberName = GetKey(method);

            // Load the node containing the parameter member.
            XmlNode xmlNode = GetNode(memberAssembly, memberName);
            if (xmlNode is null)
                throw new NullReferenceException($"Failed to find documentation for parameter '{parameter.Name}'.");
            xmlNode = xmlNode.SelectSingleNode($"param[@name='{parameter.Name}']");
            if (xmlNode is null)
                throw new NullReferenceException($"Failed to find documentation for parameter '{parameter.Name}'.");

            // Deserialize the documentation into a specified type.
            XmlSerializer xmlSerializer = new(typeof(T));
            using XmlNodeReader reader = new(xmlNode);
            return (T)xmlSerializer.Deserialize(reader);
        }
    }
}