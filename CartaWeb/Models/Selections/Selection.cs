using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CartaWeb.Models.Selections
{
    /// <summary>
    /// Represents a selection of nodes requested by a client.
    /// </summary>
    public class Selection
    {
        /// <summary>
        /// Gets or sets the identifiers of nodes that are selected from the requested set.
        /// </summary>
        /// <value>The identifiers of shallowly selected nodes.</value>
        [JsonPropertyName("shallow")]
        public List<Guid> ShallowIds { get; set; }
        /// <summary>
        /// Gets or sets the identifiers of nodes that have selected children not from the requested set.
        /// </summary>
        /// <value>The identifiers of deeply selected nodes.</value>
        [JsonPropertyName("deep")]
        public List<Guid> DeepIds { get; set; }
    }
}