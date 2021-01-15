using System;
using System.Text.Json.Serialization;

namespace CartaCore.Integration.HyperThought.Data
{
    public class FileContent
    {
        #region Naming Information
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("ftype")]
        public string FileExtension { get; set; }
        [JsonPropertyName("path")]
        public string UUIDPath { get; set; }
        [JsonPropertyName("path_string")]
        public string DirectoryPath { get; set; }
        #endregion

        #region Identity Information
        [JsonPropertyName("file")]
        public string FileId { get; set; }
        [JsonPropertyName("backend")]
        public Backend Backend { get; set; }
        [JsonPropertyName("pk")]
        public string PrimaryKey { get; set; }
        #endregion

        #region Contents Information
        [JsonPropertyName("size")]
        public int Size { get; set; }
        [JsonPropertyName("items")]
        public int FolderItems { get; set; }
        #endregion

        #region Creation Information
        [JsonPropertyName("created_by")]
        public string CreatedBy { get; set; }
        [JsonPropertyName("created")]
        public DateTime CreatedTime { get; set; }
        [JsonPropertyName("modified_by")]
        public string LastModifiedBy { get; set; }
        [JsonPropertyName("modified")]
        public DateTime LastModifiedTime { get; set; }
        #endregion
    }
}