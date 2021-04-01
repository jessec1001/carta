using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents the content of a HyperThought workflow.
    /// </summary>
    public class HyperthoughtWorkflowContent
    {
        #region Naming Information
        /// <summary>
        /// The XML that represents the content graph.
        /// </summary>
        [JsonPropertyName("xml")]
        public string Xml { get; set; }
        /// <summary>
        /// The name of the workflow.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }
        #endregion

        #region Identity Information
        /// <summary>
        /// The ID of this workflow process.
        /// </summary>
        [JsonPropertyName("pid")]
        public string ProcessId { get; set; }
        /// <summary>
        /// The ID of the parent process.
        /// </summary>
        [JsonPropertyName("parent_process")]
        public Guid? ParentProcessId { get; set; }
        /// <summary>
        /// The ID of the client.
        /// </summary>
        [JsonPropertyName("client_id")]
        public Guid? ClientId { get; set; }
        /// <summary>
        /// The IDs of processes that come after this one.
        /// </summary>
        [JsonPropertyName("successors")]
        public List<Guid> SuccessorIds { get; set; }
        /// <summary>
        /// The IDs of processes that come before this one.
        /// </summary>
        [JsonPropertyName("predecessors")]
        public List<Guid> PredecessorIds { get; set; }
        /// <summary>
        /// The IDs of children processes of this one.
        /// </summary>
        [JsonPropertyName("children")]
        public List<Guid> ChildrenIds { get; set; }
        /// <summary>
        /// The unique primary key that this workflow is stored by.
        /// </summary>
        [JsonPropertyName("pk")]
        public Guid PrimaryKey { get; set; }
        #endregion

        #region Creation Information
        /// <summary>
        /// The type of this process.
        /// </summary>
        [JsonPropertyName("process_type")]
        public HyperthoughtWorkflowType Type { get; set; }
        /// <summary>
        /// The template name of this workflow.
        /// </summary>
        [JsonPropertyName("template")]
        public bool Template { get; set; }
        /// <summary>
        /// The creator user of this workflow.
        /// </summary>
        [JsonPropertyName("creator")]
        public string CreatedBy { get; set; }
        /// <summary>
        /// The time this workflow was created.
        /// </summary>
        [JsonPropertyName("created")]
        public DateTime CreatedTime { get; set; }
        /// <summary>
        /// The last modifier user of this workflow.
        /// </summary>
        /// <value></value>
        [JsonPropertyName("modifier")]
        public string LastModifiedBy { get; set; }
        /// <summary>
        /// The last time this workflow was modified.
        /// </summary>
        [JsonPropertyName("modified")]
        public DateTime LastModifiedTime { get; set; }
        #endregion

        #region Status Information
        /// <summary>
        /// To whom this workflow has been assigned.
        /// </summary>
        [JsonPropertyName("assignee")]
        public string Assignee { get; set; }
        /// <summary>
        /// The status of this workflow.
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; }
        /// <summary>
        /// When this workflow was started.
        /// </summary>
        [JsonPropertyName("started")]
        public DateTime? StartedTime { get; set; }
        /// <summary>
        /// When this workflow was completed.
        /// </summary>
        [JsonPropertyName("completed")]
        public DateTime? CompletedTime { get; set; }
        /// <summary>
        /// The notes about this workflow.
        /// </summary>
        [JsonPropertyName("notes")]
        public string Notes { get; set; }
        #endregion
    }
}