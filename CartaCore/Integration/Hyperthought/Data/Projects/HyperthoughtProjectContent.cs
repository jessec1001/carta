using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents the content of a HyperThought project.
    /// </summary>
    public class HyperthoughtProjectContent
    {
        #region Identity Information
        /// <summary>
        /// The title of the project.
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }
        /// <summary>
        /// The alias of the project, if any.
        /// </summary>
        [JsonPropertyName("alias")]
        public string Alias { get; set; }
        /// <summary>
        /// The description of the project.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }
        /// <summary>
        /// The major outcomes of the project, if any.
        /// </summary>
        [JsonPropertyName("major_outcomes")]
        public string Outcomes { get; set; }
        /// <summary>
        /// The future work of the project, if any.
        /// </summary>
        [JsonPropertyName("future_work")]
        public string FutureWork { get; set; }
        /// <summary>
        /// A list of keywords that describe the project.
        /// </summary>
        [JsonPropertyName("keywords")]
        public List<string> Keywords { get; set; }
        /// <summary>
        /// The unique primary key that this workflow is stored by.
        /// </summary>
        [JsonPropertyName("pk")]
        public Guid PrimaryKey { get; set; }
        #endregion

        #region Creation Information
        /// <summary>
        /// The point of contact of the project.
        /// </summary>
        [JsonPropertyName("poc")]
        public string PointOfContact { get; set; }
        /// <summary>
        /// The creator user of this project.
        /// </summary>
        [JsonPropertyName("creator")]
        public string CreatedBy { get; set; }
        /// <summary>
        /// The time this project was created.
        /// </summary>
        [JsonPropertyName("created")]
        public DateTime CreatedTime { get; set; }
        #endregion

        #region Status Information
        /// <summary>
        /// The status of this project.
        /// </summary>
        [JsonPropertyName("status")]
        public HyperthoughtProcessStatus Status { get; set; }
        #endregion
    }
}