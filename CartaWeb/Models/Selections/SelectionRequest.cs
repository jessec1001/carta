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
        /// <value>The identifiers of vertices to get selection information on. May be <c>null</c>.</value>
        [JsonPropertyName("ids")]
        public List<Guid> Ids { get; set; }
        /// <summary>
        /// Gets or sets the selectors to be used.
        /// </summary>
        /// <value>The selectors to be applied to the requested identifiers.</value>
        [JsonPropertyName("selectors")]
        public List<SelectorBase> Selectors { get; set; }
        /// <summary>
        /// Gets or sets whether the returned selection should be condensed to the specified identifiers.
        /// </summary>
        /// <value>
        /// <c>true</c> if specified identifiers should have the nested property assigned to them rather than computing
        /// their children selections. <c>false</c> if all selected nodes should be returned.
        /// </value>
        [JsonPropertyName("condensed")]
        public bool Condensed { get; set; }
    }
}