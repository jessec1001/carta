using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents information about the type and value of data in a HyperThought object.
    /// </summary>
    public class HyperthoughtMetadata
    {
        /// <summary>
        /// The key name of the data.
        /// </summary>
        [JsonPropertyName("keyName")]
        public string Key { get; set; }
        /// <summary>
        /// The value of the data.
        /// </summary>
        [JsonPropertyName("value")]
        public HyperthoughtMetadataValue Value { get; set; }
        /// <summary>
        /// Notes and units of the data.
        /// </summary>
        /// <value></value>
        [JsonPropertyName("annotation")]
        public string Annotation { get; set; }
        /// <summary>
        /// Collection of any other key/value pairs not explicitly
        /// included in the formal schema.
        /// </summary>
        // This same Property is present in HyperthoughtMetadata.
        // An abstraction would be an appropriate way to ensure the
        // interface remains consistent, but at the moment this appears
        // in only these two locations. If this extends to other
        // containers in the future, this should be abstracted into
        // an interface.
        [JsonExtensionData]
        public Dictionary<string, object> Extensions { get; set; }
    }
}