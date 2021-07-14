using System;
using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents the content of a HyperThought group.
    /// </summary>
    public class HyperthoughtGroupContent
    {
        #region Identity Information
        /// <summary>
        /// The name of the group.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }
        /// <summary>
        /// The alias of the group.
        /// </summary>
        [JsonPropertyName("alias")]
        public string Alias { get; set; }
        /// <summary>
        /// The unique primary key that this group is stored by.
        /// </summary>
        [JsonPropertyName("pk")]
        public Guid PrimaryKey { get; set; }
        #endregion

        #region Creation Information
        /// <summary>
        /// The time this group was created.
        /// </summary>
        [JsonPropertyName("created")]
        public DateTime CreatedTime { get; set; }
        #endregion
    }
}