using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    // TODO: This is a DTO not a standard data structure.
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

    // TODO: This is a DTO not a standard data structure.
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
