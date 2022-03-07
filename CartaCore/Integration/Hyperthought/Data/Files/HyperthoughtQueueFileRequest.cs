using System;
using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought
{
    // TODO: This is a DTO not a standard data structure.
    /// <summary>
    /// Represents a request to queue a file for moving in Hyperthought
    /// </summary>
    public class HyperthoughtQueueFileRequest
    {
        /// <summary>
        /// The type of queue request
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }
        /// <summary>
        /// The UUID of the file to move
        /// </summary>
        [JsonPropertyName("fromUuid")]
        public Guid FromFileId { get; set; }
        /// <summary>
        /// The UUID of the directory to move the file to
        /// </summary>
        [JsonPropertyName("toUuid")]
        public Guid ToDirectoryId { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="HyperthoughtQueueFileRequest"/> class
        /// </summary>
        public HyperthoughtQueueFileRequest() { Type = "move"; }
    }
}
