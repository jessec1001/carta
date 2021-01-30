using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents a HyperThought file with its content, metadata, and structure.
    /// </summary>
    public class HyperthoughtFile : HyperthoughtObjectBase
    {
        /// <summary>
        /// A value containing the file content.
        /// </summary>
        [JsonPropertyName("content")]
        public HyperthoughtFileContent Content { get; set; }
    }
}