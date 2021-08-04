using System;
using CartaWeb.Models.Data;

namespace CartaWeb.Models.DocumentItem
{
    /// <summary>
    /// Represents user information stored at a workspace level
    /// </summary>
    public class UserItem
    {
        /// <summary>
        /// The user information
        /// </summary>
        public UserInformation UserInformation { get; set; }
        /// <summary>
        /// History of the user item
        /// </summary>
        public DocumentHistory DocumentHistory { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="UserItem"/> class
        /// </summary>
        public UserItem (UserInformation userInformation)
        {
            UserInformation = userInformation;
            DocumentHistory = new DocumentHistory(userInformation);
        }

    }
}
 