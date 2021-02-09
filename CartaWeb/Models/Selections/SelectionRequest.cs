using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using CartaCore.Workflow.Selection;

namespace CartaWeb.Models.Selections
{
    /// <summary>
    /// Represents a selection request from a client.
    /// </summary>
    public class SelectionRequest
    {
        /// <summary>
        /// Gets or sets the identifiers that are requested. This field is optional and if not specified will request
        /// identifiers on the entire graph.
        /// </summary>
        /// <value>The identifiers of vertices to get selection information on.</value>
        [JsonPropertyName("ids")]
        public List<Guid> Ids { get; set; }
        /// <summary>
        /// Gets or sets the selectors to be used.
        /// </summary>
        /// <value>The selectors to be applied to the requested identifiers.</value>
        [JsonPropertyName("selectors")]
        public List<SelectorBase> Selectors { get; set; }
    }
}