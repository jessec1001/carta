using System.Text.Json.Serialization;

namespace CartaCore.Integration.Hyperthought.Data
{
    /// <summary>
    /// Represents resources used by workspace.
    /// </summary>
    public class HyperthoughtWorkspaceResource
    {
        /// <summary>
        /// The API resource ID used to access this workspace.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }
    }
}