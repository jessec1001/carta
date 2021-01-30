using System;
using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents a HyperThought workflow template.
    /// </summary>
    public class HyperthoughtWorkflowTemplate
    {
        /// <summary>
        /// The title of the workflow template.
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }
        /// <summary>
        /// The unique primary key that this workflow is stored by.
        /// </summary>
        [JsonPropertyName("key")]
        public Guid PrimaryKey { get; set; }
        /// <summary>
        /// Whether the template is lazily-loaded.
        /// </summary>
        [JsonPropertyName("lazy")]
        public bool Lazy { get; set; }

        /// <summary>
        /// The creator user of this workflow template.
        /// </summary>
        [JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; }
        /// <summary>
        /// The creator name of this workflow template.
        /// </summary>
        [JsonPropertyName("createdByFullName")]
        public string CreatedByName { get; set; }
        /// <summary>
        /// The time this workflow template was created.
        /// </summary>
        [JsonPropertyName("createdOn")]
        public DateTime CreatedTime { get; set; }
        /// <summary>
        /// The last modifier user of this workflow template.
        /// </summary>
        [JsonPropertyName("modifiedBy")]
        public string LastModifiedBy { get; set; }
        /// <summary>
        /// The last modifier name of this workflow template.
        /// </summary>
        [JsonPropertyName("modifiedByFullName")]
        public string LastModifiedByName { get; set; }
        /// <summary>
        /// The last time this workflow template was modified.
        /// </summary>
        [JsonPropertyName("modifiedOn")]
        public DateTime LastModifiedTime { get; set; }
    }
}