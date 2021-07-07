using CartaWeb.Models.Meta;

namespace CartaWeb.Models.Data
{
    /// <summary>
    /// An enumeration that defines actions taken in a workspace
    /// </summary>
    [ApiType(typeof(string))]
    public enum WorkspaceActionEnumeration
    {
        /// <summary>
        /// An object was added to a workspace
        /// </summary>
        Added,
        /// <summary>
        /// An object was removed from a workspace
        /// </summary>
        Removed,
        /// <summary>
        /// An object was updated
        /// </summary>
        Updated
    }
}
