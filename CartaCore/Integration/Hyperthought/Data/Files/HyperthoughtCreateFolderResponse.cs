using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents a Hyperthought create folder response
    /// </summary>
    public class HyperthoughtCreateFolderResponse : HyperthoughtObjectBase
    {
        /// <summary>
        /// The HyperThought file content
        /// </summary>
        [JsonPropertyName("document")]
        public HyperthoughtCreateFolderResponseDocument Document { get; set; }
        /// <summary>
        /// A message from Hyperthought 
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    /// <summary>
    /// Represents the document Hyperthought create folder response
    /// </summary>
    public class HyperthoughtCreateFolderResponseDocument : HyperthoughtObjectBase
    {
        /// <summary>
        /// The HyperThought file content
        /// </summary>
        [JsonPropertyName("content")]
        public HyperthoughtFileContent Content { get; set; }
    }
}
