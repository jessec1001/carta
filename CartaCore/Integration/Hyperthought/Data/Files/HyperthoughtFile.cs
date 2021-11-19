using System;
using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents a HyperThought file with its content, metadata, and structure.
    /// </summary>
    public class HyperthoughtFile : HyperthoughtObjectBase
    {

        #region Naming Information
        /// <summary>
        /// The name of the file.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }
        /// <summary>
        /// The extension of this file or <c>"Folder"</c> if it is a directory.
        /// </summary>
        [JsonPropertyName("ftype")]
        public string FileExtension { get; set; }
        /// <summary>
        /// The path of the file's parent directory by UUID comma-separated list.
        /// </summary>
        /// <value></value>
        [JsonPropertyName("path")]
        public string UUIDPath { get; set; }
        /// <summary>
        /// The path of the file by human-readable directory structure.
        /// </summary>
        [JsonPropertyName("path_string")]
        public string DirectoryPath { get; set; }
        /// <summary>
        /// The name of the file's workspace
        /// </summary>
        [JsonPropertyName("workspaceName")]
        public string WorkspaceName { get; set; }
        #endregion

        #region Identity Information
        /// <summary>
        /// The ID of the file.
        /// </summary>
        [JsonPropertyName("file")]
        public Guid FileId { get; set; }
        /// <summary>
        /// How the file is stored.
        /// </summary>
        [JsonPropertyName("backend")]
        public HyperthoughtFileBackend Backend { get; set; }
        /// <summary>
        /// The unique primary key that this file is stored by.
        /// </summary>
        [JsonPropertyName("pk")]
        public Guid PrimaryKey { get; set; }
        /// <summary>
        /// The canonical URI of the file
        /// </summary>
        [JsonPropertyName("canonicalUri")]
        public string CanonicalUri { get; set; }
        /// <summary>
        /// The process ID of the file.
        /// </summary>
        [JsonPropertyName("pid")]
        public Guid ProcessId { get; set; }
        #endregion

        #region Creation Information
        /// <summary>
        /// The creator user of this file.
        /// </summary>
        [JsonPropertyName("created_by")]
        public string CreatedBy { get; set; }
        /// <summary>
        /// The time this file was created.
        /// </summary>
        [JsonPropertyName("created")]
        public DateTime CreatedTime { get; set; }
        /// <summary>
        /// The last modifier user of this file.
        /// </summary>
        [JsonPropertyName("modified_by")]
        public string LastModifiedBy { get; set; }
        /// <summary>
        /// The last time this file was modified.
        /// </summary>
        [JsonPropertyName("modified")]
        public DateTime LastModifiedTime { get; set; }
        #endregion

        #region Contents Information
        /// <summary>
        /// The size, in bytes, of the file; or, if this is a directory, the size of its contents.
        /// </summary>
        [JsonPropertyName("size")]
        public int Size { get; set; }
        /// <summary>
        /// The number of items contained in this if it is a directory.
        /// </summary>
        [JsonPropertyName("items")]
        public int FolderItems { get; set; }
        #endregion

        #region Restriction Information
        /// <summary>
        /// The value of the distribution the data is restricted to.
        /// </summary>
        [JsonPropertyName("distributionLevel")]
        public HyperthoughtDistribution Distribution { get; set; }
        /// <summary>
        /// The value of the export the data is restricted to.
        /// </summary>
        [JsonPropertyName("exportControl")]
        public HyperthoughtExportControl ExportControl { get; set; }
        /// <summary>
        /// The value of the security marking the data is restricted to.
        /// </summary>
        /// <value></value>
        [JsonPropertyName("securityMarking")]
        public string SecurityMarking { get; set; }
        #endregion

        /// <summary>
        /// Resources associated with the file
        /// </summary>
        [JsonPropertyName("resources")]
        public HyperthoughtFileResource Resources { get; set; }
        
    }
}