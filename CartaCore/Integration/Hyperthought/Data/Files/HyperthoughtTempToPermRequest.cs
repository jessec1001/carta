using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought
{
    // TODO: This is a DTO not a standard data structure.
    /// <summary>
    /// Represents a request to move a list of files to permanent storage
    /// </summary>
    public class HyperthoughtTempToPermRequest
    {
        /// <summary>
        /// A list of file UUIDs to make permanent
        /// </summary>
        [JsonPropertyName("file_ids")]
        public List<Guid> FileIds { get; set; }

    }
}
