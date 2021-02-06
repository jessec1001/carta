using System.Collections.Generic;

using CartaCore.Data;

namespace CartaCore.Workflow
{
    /// <summary>
    /// Represents a semantic map in which some properties of data are overwritten by others.
    /// </summary>
    public class SemanticMappingTransform
    {
        /// <summary>
        /// The mapping of semantic overrides.
        /// </summary>
        /// <value>Each key is the overridden property and each value is the overriding property.</value>
        public Dictionary<string, string> Mapping { get; set; }

        /// <summary>
        /// Creates a new semantic mapping transformation.
        /// </summary>
        public SemanticMappingTransform()
            : this(new Dictionary<string, string>()) { }
        /// <summary>
        /// Creates a new semantic mapping transformation with the specified mappings.
        /// </summary>
        /// <param name="mapping">The mapping of semantic overrides.</param>
        public SemanticMappingTransform(Dictionary<string, string> mapping)
        {
            Mapping = mapping;
        }

        /// <summary>
        /// Transforms the specified vertex based on the semantic mapping.
        /// </summary>
        /// <remarks>
        /// Semantic overrides are specified in <see cref="Mapping"/> where the key represents the overridden property
        /// and the value represents the overriding property. The overriding property name always replaces the
        /// overridden property name. The overriding property value only replaces the overridden property name if both
        /// properties exist on the vertex. Otherwise, no transformation is performed.
        /// </remarks>
        /// <param name="vertex">The vertex to transform. Will be unchanged.</param>
        /// <returns>The transformed vertex.</returns>
        public FreeformVertex Transform(FreeformVertex vertex)
        {
            // We can't operate if we don't have a mapping.
            if (Mapping is null) return vertex;

            // Create the new properties list to store the transformed properties.
            // We do this instead of operating in-place because data should be immutable.
            SortedList<string, FreeformProperty> properties =
                new SortedList<string, FreeformProperty>(vertex.Properties.Count);

            foreach (KeyValuePair<string, FreeformProperty> pair in vertex.Properties)
            {
                if (Mapping.TryGetValue(pair.Key, out string overriding))
                {
                    // If both properties are attached, ignore the overridden one.
                    if (!vertex.Properties.ContainsKey(overriding))
                    {
                        // We override the original property if it has a semantic override.
                        properties.Add(overriding, pair.Value);
                    }
                }
                else
                {
                    // We don't transform if the property doesn't have a semantic override.
                    properties.Add(pair.Key, pair.Value);
                }
            }

            // We clear empty capacity on the property list to optimize memory usage.
            properties.Capacity = properties.Count;

            // Return a new vertex with the transformed properties.
            return new FreeformVertex
            {
                Id = vertex.Id,
                Properties = properties
            };
        }
    }
}