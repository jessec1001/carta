using CartaWeb.Models.Meta;

namespace CartaWeb.Models.Data
{
    /// <summary>
    /// An enumeration that defines changes made in a workspace
    /// </summary>
    [ApiType(typeof(string))]
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
        Worklow,
        /// <summary>
        /// Addition, update or removal of a dataset
        /// </summary>
        Dataset
    }
}
