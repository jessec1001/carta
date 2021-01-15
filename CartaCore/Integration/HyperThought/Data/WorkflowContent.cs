using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CartaCore.Integration.HyperThought.Data
{
    public class WorkflowContent
    {
        #region Naming Information
        [JsonPropertyName("xml")]
        public string Xml { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        #endregion

        #region Identity Information
        [JsonPropertyName("pid")]
        public string ProcessId { get; set; }
        [JsonPropertyName("parent_process")]
        public string ParentProcessId { get; set; }
        [JsonPropertyName("client_id")]
        public string ClientId { get; set; }
        [JsonPropertyName("successors")]
        public List<string> SuccessorIds { get; set; }
        [JsonPropertyName("predecessors")]
        public List<string> PredecessorIds { get; set; }
        [JsonPropertyName("children")]
        public List<string> ChildrenIds { get; set; }
        [JsonPropertyName("pk")]
        public string PrimaryKey { get; set; }
        #endregion

        #region Creation Information
        [JsonPropertyName("process_type")]
        public ProcessType Type { get; set; }
        [JsonPropertyName("template")]
        public string Template { get; set; }
        [JsonPropertyName("creator")]
        public string CreatedBy { get; set; }
        [JsonPropertyName("created")]
        public DateTime CreatedTime { get; set; }
        [JsonPropertyName("modifier")]
        public string LastModifiedBy { get; set; }
        [JsonPropertyName("modified")]
        public DateTime LastModifiedTime { get; set; }
        #endregion

        #region Status Information
        [JsonPropertyName("assignee")]
        public string Assignee { get; set; }
        [JsonPropertyName("status")]
        public WorkflowStatus Status { get; set; }
        [JsonPropertyName("started")]
        public DateTime StartedTime { get; set; }
        [JsonPropertyName("completed")]
        public DateTime CompletedTime { get; set; }
        [JsonPropertyName("notes")]
        public string Notes { get; set; }
        #endregion
    }
}