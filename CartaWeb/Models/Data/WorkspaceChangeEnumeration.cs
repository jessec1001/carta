using CartaWeb.Models.Meta;
using System.Text.Json.Serialization;

namespace CartaWeb.Models.Data
{
    /// <summary>
    /// An enumeration that defines changes made in a workspace
    /// </summary>
    [ApiType(typeof(string))]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum WorkspaceChangeEnumeration
    {
        /// <summary>
        /// Addition of a workspace
        /// </summary>
        Workspace,
        /// <summary>
        /// Addition or removal of a user
        /// </summary>
        User,
        /// <summary>
        /// Addition, update or removal of a workflow
        /// </summary>
        Workflow,
        /// <summary>
        /// Addition, update or removal of a dataset
        /// </summary>
        Dataset
    }
}
