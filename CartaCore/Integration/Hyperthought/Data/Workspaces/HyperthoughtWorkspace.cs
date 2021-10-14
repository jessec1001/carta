using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents the content of a HyperThought workspace.
    /// </summary>
    public class HyperthoughtWorkspace
    {
        #region Identity Information
        /// <summary>
        /// The name of the workspace.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }
        /// <summary>
        /// The alias of the workspace, if any.
        /// </summary>
        [JsonPropertyName("alias")]
        public string Alias { get; set; }
        /// <summary>
        /// The description of the workspace.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }
        /// <summary>
        /// The major outcomes of the workspace, if any.
        /// </summary>
        [JsonPropertyName("majorOutcomes")]
        public string Outcomes { get; set; }
        /// <summary>
        /// The future work of the workspace, if any.
        /// </summary>
        [JsonPropertyName("futureWork")]
        public string FutureWork { get; set; }
        /// <summary>
        /// A list of keywords that describe the workspace.
        /// </summary>
        [JsonPropertyName("keywords")]
        public List<string> Keywords { get; set; }
        /// <summary>
        /// The unique primary key that this workflow is stored by.
        /// </summary>
        [JsonPropertyName("id")]
        public Guid PrimaryKey { get; set; }
        #endregion

        #region Contents
        /// <summary>
        /// Metadata associated with this workspace, if any.
        /// </summary>
        [JsonPropertyName("metadata")]
        public List<HyperthoughtMetadata> Metadata { get; set; }
        /// <summary>
        /// Files associated with this workspace, if any.
        /// </summary>
        [JsonPropertyName("files")]
        public List<string> Files { get; set; }
        /// <summary>
        /// Resources associated with the workspace, if any.
        /// </summary>
        [JsonPropertyName("resources")]
        public HyperthoughtWorkspaceResource Resources { get; set; }
        #endregion

        #region Creation Information
        /// <summary>
        /// The point of contact of the workspace.
        /// </summary>
        [JsonPropertyName("poc")]
        public string PointOfContact { get; set; }
        /// <summary>
        /// The creator user of this workspace.
        /// </summary>
        [JsonPropertyName("creator")]
        public string CreatedBy { get; set; }
        /// <summary>
        /// The time this workspace was created.
        /// </summary>
        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }
        #endregion

        #region Status Information
        /// <summary>
        /// The status of this workspace.
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; }
        /// <summary>
        /// The distribution statement for this workspace.
        /// </summary>
        [JsonPropertyName("distributionStatement")]
        public string DistributionStatement { get; set; }
        /// <summary>
        /// Users with access to this workspace.
        /// </summary>
        [JsonPropertyName("members")]
        public List<HyperthoughtWorkspaceMember> Members { get; set; }
        #endregion
    }
}