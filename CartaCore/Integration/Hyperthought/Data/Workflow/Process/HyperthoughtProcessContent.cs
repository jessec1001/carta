using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents the content of a HyperThought process.
    /// </summary>
    public class HyperthoughtProcessContent
    {
        /// <summary>
        /// Initializes an instance of the <see cref="HyperthoughtProcessContent"/> class.
        /// </summary>
        /// <remarks>This is the blank constructor.</remarks>
        public HyperthoughtProcessContent() { }
        /// <summary>
        /// Initializes an instance of the <see cref="HyperthoughtProcessContent"/> class from an existing instance.
        /// </summary>
        /// <remarks>This is a copy constructor.</remarks>
        public HyperthoughtProcessContent(HyperthoughtProcessContent content)
        {
            Xml = content.Xml;
            Name = content.Name;

            ProcessId = content.ProcessId;
            ParentProcessId = content.ParentProcessId;
            ClientId = content.ClientId;
            SuccessorIds = content.SuccessorIds;
            PredecessorIds = content.PredecessorIds;
            ChildrenIds = content.ChildrenIds;
            PrimaryKey = content.PrimaryKey;

            Type = content.Type;
            Template = content.Template;
            CreatedBy = content.CreatedBy;
            CreatedTime = content.CreatedTime;
            LastModifiedBy = content.LastModifiedBy;
            LastModifiedTime = content.LastModifiedTime;
            Assignee = content.Assignee;
            Status = content.Status;
            StartedTime = content.StartedTime;
            CompletedTime = content.CompletedTime;
            Notes = content.Notes;

            Extensions = content.Extensions;
        }

        #region Naming Information
        /// <summary>
        /// The XML that represents the content graph.
        /// </summary>
        [JsonPropertyName("xml")]
        public string Xml { get; set; }
        /// <summary>
        /// The name of the process.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }
        #endregion

        #region Identity Information
        /// <summary>
        /// The ID of this process.
        /// </summary>
        [JsonPropertyName("pid")]
        public Guid? ProcessId { get; set; }
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
        /// The unique primary key that this process is stored by.
        /// </summary>
        [JsonPropertyName("pk")]
        public Guid PrimaryKey { get; set; }
        #endregion

        #region Creation Information
        /// <summary>
        /// The type of this process.
        /// </summary>
        [JsonPropertyName("process_type")]
        public HyperthoughtProcessType Type { get; set; }
        /// <summary>
        /// Whether the process is a template workflow or not.
        /// </summary>
        [JsonPropertyName("template")]
        public bool Template { get; set; }
        /// <summary>
        /// The creator user of this process.
        /// </summary>
        [JsonPropertyName("creator")]
        public string CreatedBy { get; set; }
        /// <summary>
        /// The time this process was created.
        /// </summary>
        [JsonPropertyName("created")]
        public DateTime CreatedTime { get; set; }
        /// <summary>
        /// The last modifier user of this process.
        /// </summary>
        [JsonPropertyName("modifier")]
        public string LastModifiedBy { get; set; }
        /// <summary>
        /// The last time this process was modified.
        /// </summary>
        [JsonPropertyName("modified")]
        public DateTime LastModifiedTime { get; set; }
        #endregion

        #region Status Information
        /// <summary>
        /// To whom this process has been assigned.
        /// </summary>
        [JsonPropertyName("assignee")]
        public string Assignee { get; set; }
        /// <summary>
        /// The status of this process.
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; }
        /// <summary>
        /// When this process was started.
        /// </summary>
        [JsonPropertyName("started")]
        public DateTime? StartedTime { get; set; }
        /// <summary>
        /// When this process was completed.
        /// </summary>
        [JsonPropertyName("completed")]
        public DateTime? CompletedTime { get; set; }
        /// <summary>
        /// The notes about this process.
        /// </summary>
        [JsonPropertyName("notes")]
        public string Notes { get; set; }
        #endregion

        /// <summary>
        /// Collection of any other key-value pairs not explicitly included in the formal schema.
        /// </summary>
        /// <remarks>
        /// This same property is present in <see cref="HyperthoughtMetadata"/>. An abstraction would be an appropriate
        /// way to ensure the interface remains consistent, but at the moment this appears in only these two locations.
        /// If this extends to other containers in the future, this should be abstracted into a base class.
        /// </remarks>
        [JsonExtensionData]
        public Dictionary<string, object> Extensions { get; set; }
    }
}