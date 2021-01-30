using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents a generic HyperThought object.
    /// </summary>
    public abstract class HyperthoughtObjectBase
    {
        /// <summary>
        /// A value containing all of the data triples.
        /// </summary>
        [JsonPropertyName("triples")]
        public List<HyperthoughtTriple> Triples { get; set; }
        /// <summary>
        /// A value containing metadata about the types of data.
        /// </summary>
        [JsonPropertyName("metadata")]
        public List<HyperthoughtMetadata> Metadata { get; set; }
        /// <summary>
        /// A value containing information about creation and modification.
        /// </summary>
        [JsonPropertyName("headers")]
        public HyperthoughtHeaders Headers { get; set; }
        /// <summary>
        /// A value containing information about which users, groups, and projects have which permissions.
        /// </summary>
        [JsonPropertyName("permissions")]
        public HyperthoughtPermissions Permissions { get; set; }
        /// <summary>
        /// A value containing information about where this is restricted to.
        /// </summary>
        [JsonPropertyName("restrictions")]
        public HyperthoughtRestrictions Restrictions { get; set; }
    }
}