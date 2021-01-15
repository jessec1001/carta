using System;

using QuikGraph;

namespace CartaWeb.Mappings
{
    public enum DataNamespace
    {
        Synthetic,
        HyperThought
    }

    public static class DataMapping
    {
        static DataMapping()
        {

        }

        public static IUndirectedGraph<int, Edge<int>> Resolve(string id)
        {
            // Break the identity string into parts.
            // Identity strings are of the form "namespace:name".
            string[] parts = id.Split(':');
            if (parts.Length != 2)
                return null;
            string namespaceId = parts[0];
            string nameId = parts[1];

            // Get the data namespace. If no corresponding one is found, return null.
            bool parsedNamespaceId = Enum.TryParse<DataNamespace>(namespaceId, true, out DataNamespace dataNamespace);
            if (parsedNamespaceId)
                return null;

            // Get the data name. If no corresponding one is found, return null.
        }
    }
}