using CartaWeb.Models.Meta;

namespace CartaWeb.Models.Data
{
    /// <summary>
    /// An enumeration that defines user attribute constants
    /// </summary>
    [ApiType(typeof(string))]
    public enum UserAttributeEnumeration
    {
        /// <summary>
        /// The user identifier
        /// </summary>
        UserId,
        /// <summary>
        /// The user name
        /// </summary>
        UserName,
        /// <summary>
        /// The user's email
        /// </summary>
        Email,
        /// <summary>
        /// The user's first (given) name
        /// </summary>
        FirstName,
        /// <summary>
        /// The user's last name
        /// </summary>
        LastName
    }
}
