using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;

namespace CartaWeb.Models.Meta
{
    /// <summary>
    /// A set of extensions that retrieve documentation for the source code.
    /// </summary>
    public static class ApiDocumentationExtensions
    {
        private static HashSet<Assembly> LoadedAssemblies = new HashSet<Assembly>();
        private static Dictionary<string, XmlNode> LoadedMembers = new Dictionary<string, XmlNode>();

        /// <summary>
        /// Gets the path to the documentation of an assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>The documentation path.</returns>
        public static string GetDocsPath(this Assembly assembly)
        {
            string assemblyPath = assembly.Location;
            string assemblyDirectory = Path.GetDirectoryName(assemblyPath);
            string documentationPath = Path.Combine(assemblyDirectory, $"{assembly.GetName().Name}.xml");
            return documentationPath;
        }
        /// <summary>
        /// Loads the documentation members of an assembly with an optionally specified documentation path.
        /// </summary>
        /// <param name="assembly">The assembly to consume the documentation of.</param>
        /// <param name="path">The assembly path. Optional.</param>
        public static void LoadDocs(this Assembly assembly, string path = null)
        {
            // Check if the documentation is already loaded.
            if (LoadedAssemblies.Contains(assembly)) return;
            LoadedAssemblies.Add(assembly);

            // Load in the XML documentation file.
            path = path ?? assembly.GetDocsPath();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            foreach (XmlNode xmlMember in xmlDoc.GetElementsByTagName("member"))
                LoadedMembers.Add(xmlMember.Attributes?.GetNamedItem("name")?.InnerText, xmlMember);
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
        private static string DocumentationGetTypeName(Type type)
        {
            if (type.IsGenericType)
            {
                string genericArgumentNames = string.Join
                (',', type
                    .GetGenericArguments()
                    .Select(genericType => DocumentationGetTypeName(genericType))
                );
                string genericTypeName = type.FullName.Substring
                    (0, type.FullName.IndexOf('`'));
                return $"{genericTypeName}{{{genericArgumentNames}}}";
            }
            else return type.FullName;
        }
        /// <summary>
        /// Selects the text of child elements by an XPath path.
        /// </summary>
        /// <param name="node">The node to select text from.</param>
        /// <param name="path">The XPath path to child elements.</param>
        /// <returns>An enumerable of the text inside the selected elements.</returns>
        private static IEnumerable<string> DocumentationNodeSelector(XmlNode node, string path)
        {
            if (node is null) yield break;
            if (path is null) yield return node.InnerText;
            foreach (XmlNode selectedNode in node.SelectNodes(path))
                yield return selectedNode.InnerText;
        }
        /// <summary>
        /// Cleans a text string so that it does not contain excess whitespace.
        /// </summary>
        /// <param name="text">The text to clean.</param>
        /// <returns>The cleaned text.</returns>
        private static string DocumentationCleanString(string text)
        {
            if (text is null) return text;
            return Regex.Replace(text, @"\s+", " ").Trim();
        }

        /// <summary>
        /// Gets the XML key for a specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The key within the XML member documentation.</returns>
        private static string GetKey(Type type) => $"T:{DocumentationKeyFormatter(type.FullName, null)}";
        /// <summary>
        /// Gets the XML key for the specified method info.
        /// </summary>
        /// <param name="method">The method info.</param>
        /// <returns>The key within the XML member documentation.</returns>
        private static string GetKey(MethodInfo method)
        {
            string paramsKey = String.Join(',', method
                .GetParameters()
                .Select
                (param => DocumentationKeyFormatter
                    (DocumentationGetTypeName(param.ParameterType), null)
                )
            );
            if (String.IsNullOrEmpty(paramsKey))
                return $"M:{DocumentationKeyFormatter(method.DeclaringType.FullName, method.Name)}";
            else
                return $"M:{DocumentationKeyFormatter(method.DeclaringType.FullName, method.Name)}({paramsKey})";
        }

        /// <summary>
        /// Gets the XML documentation node of the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The XML documentation.</returns>
        public static XmlNode GetDocs(this Type type)
        {
            if (!LoadedAssemblies.Contains(type.Assembly))
                LoadDocs(type.Assembly);

            LoadedMembers.TryGetValue(GetKey(type), out XmlNode node);
            return node;
        }
        /// <summary>
        /// Gets the XML documentation node of the specified method info.
        /// </summary>
        /// <param name="method">The method info.</param>
        /// <returns>The XML documentation.</returns>
        public static XmlNode GetDocs(this MethodInfo method)
        {
            if (!LoadedAssemblies.Contains(method.DeclaringType.Assembly))
                LoadDocs(method.DeclaringType.Assembly);

            LoadedMembers.TryGetValue(GetKey(method), out XmlNode node);
            return node;
        }
        /// <summary>
        /// Gets the XML documentation node of the specified parameter info.
        /// </summary>
        /// <param name="parameter">The parameter info.</param>
        /// <returns>The XML documentation,</returns>
        public static XmlNode GetDocs(this ParameterInfo parameter)
        {
            MethodInfo method = (MethodInfo)parameter.Member;
            XmlNode methodDocs = method.GetDocs();

            if (methodDocs is null) return null;
            return methodDocs.SelectSingleNode($"param[@name='{parameter.Name}']");
        }

        /// <summary>
        /// Gets the summary documentation for the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The summary.</returns>
        public static string GetDocsSummary(this Type type)
        {
            return DocumentationCleanString(DocumentationNodeSelector(type.GetDocs(), "summary").FirstOrDefault());
        }
        /// <summary>
        /// Gets the summary documentation for the specified method info.
        /// </summary>
        /// <param name="method">The method info.</param>
        /// <returns>The summary.</returns>
        public static string GetDocsSummary(this MethodInfo method)
        {
            return DocumentationCleanString(DocumentationNodeSelector(method.GetDocs(), "summary").FirstOrDefault());
        }
        /// <summary>
        /// Gets the summary documentation for the specified parameter info.
        /// </summary>
        /// <param name="parameter">The parameter info.</param>
        /// <returns>The summary.</returns>
        public static string GetDocsSummary(this ParameterInfo parameter)
        {
            return DocumentationCleanString(parameter.GetDocs()?.InnerText);
        }

        /// <summary>
        /// Gets the returns documentation, sorted by status, of the specified method info.
        /// </summary>
        /// <param name="method">The method info.</param>
        /// <returns>The returns documentation.</returns>
        public static Dictionary<int, string> GetDocsReturns(this MethodInfo method)
        {
            XmlNode methodDocs = method.GetDocs();

            if (methodDocs is null) return null;
            Dictionary<int, string> returns = new Dictionary<int, string>();
            foreach (XmlNode returnsNode in methodDocs.SelectNodes("returns"))
            {
                // Check for a status code.
                XmlNode statusNode = returnsNode.Attributes.GetNamedItem("status");
                int status = 0;
                if (statusNode is not null) int.TryParse(statusNode.InnerText, out status);

                returns.Add
                (
                    status, DocumentationCleanString(returnsNode.InnerText)
                );
            }
            return returns;
        }

        /// <summary>
        /// Gets the requests documentation of the specified method info.
        /// </summary>
        /// <param name="method">The method info.</param>
        /// <returns>The requests documentation.</returns>
        public static List<ApiRequest> GetDocsRequests(this MethodInfo method)
        {
            XmlNode methodDocs = method.GetDocs();

            if (methodDocs is null) return null;
            List<ApiRequest> requests = new List<ApiRequest>();
            foreach (XmlNode requestNode in methodDocs.SelectNodes("request"))
            {
                // Check for a name.
                XmlNode nameNode = requestNode.Attributes.GetNamedItem("name");
                string name = nameNode?.InnerText;

                // Check for a body.
                XmlNode bodyNode = requestNode.SelectSingleNode("body");
                JsonDocument bodyJson = null;
                if (bodyNode is not null)
                {
                    bodyJson = JsonSerializer.Deserialize<JsonDocument>
                    (
                        bodyNode.InnerText,
                        new JsonSerializerOptions(JsonSerializerDefaults.Web)
                    );
                }

                // Check for arguments.
                Dictionary<string, string> arguments = new Dictionary<string, string>();
                foreach (XmlNode argNode in requestNode.SelectNodes("arg"))
                {
                    // Check for an argument name.
                    XmlNode argNameNode = argNode.Attributes.GetNamedItem("name");
                    string argName = argNameNode?.InnerText;

                    arguments.Add(argName, argNode.InnerText);
                }

                requests.Add
                (
                    new ApiRequest
                    {
                        Name = name,
                        Body = bodyJson,
                        Arguments = arguments
                    }
                );
            }
            return requests;
        }
    }
}