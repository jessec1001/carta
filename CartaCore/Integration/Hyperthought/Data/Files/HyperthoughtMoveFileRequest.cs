using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought
{
    // TODO: This is a DTO not a standard data structure.
    /// <summary>
    /// Represents a request to move files in Hyperthought
    /// </summary>
    public class HyperthoughtMoveFileRequest
    {
        /// <summary>
        /// The source workspace UUID 
        /// </summary>
        [JsonPropertyName("sourceSpaceId")]
        public Guid SourceSpaceId { get; set; }
        /// <summary>
        /// The source parent folder UUID 
        /// </summary>
        [JsonPropertyName("sourceParentFolderId")]
        public string SourceParentFolderId { get; set; }
        /// <summary>
        /// The destination workspace UUID 
        /// </summary>
        [JsonPropertyName("destinationSpaceId")]
        public Guid DestinationSpaceId { get; set; }
        /// <summary>
        /// The destination parent folder UUID 
        /// </summary>
        [JsonPropertyName("destinationParentFolderId")]
        public string DestinationParentFolderId { get; set; }
        /// <summary>
        /// A list of file UUIDs to move
        /// </summary>
        [JsonPropertyName("fileIds")]
        public List<Guid> FileIds { get; set; }


    }
}
