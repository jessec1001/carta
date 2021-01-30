using System;
using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents the content of a HyperThought file.
    /// </summary>
    public class HyperthoughtFileContent
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
        /// The path of the file by UUID comma-separated list.
        /// </summary>
        /// <value></value>
        [JsonPropertyName("path")]
        public string UUIDPath { get; set; }
        /// <summary>
        /// The path of the file by human-readable directory structure.
        /// </summary>
        [JsonPropertyName("path_string")]
        public string DirectoryPath { get; set; }
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
        public HyperthoughtBackend Backend { get; set; }
        /// <summary>
        /// The unique primary key that this file is stored by.
        /// </summary>
        [JsonPropertyName("pk")]
        public Guid PrimaryKey { get; set; }
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
    }
}