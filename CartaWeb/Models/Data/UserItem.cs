using System;

namespace CartaWeb.Models.Data
{
    /// <summary>
    /// Represents user information stored at a workspace level
    /// </summary>
    public class UserItem
    {
        /// <summary>
        /// The user ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The user name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The user's email
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// The group selected for sharing the user
        /// </summary>
        public string Group { get; set; }
        /// <summary>
        /// Timestamp of the date the user was added
        /// </summary>
        public DateTime? DateAdded { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="UserItem"/> class with a specified controller.
        /// </summary>
        public UserItem (string id, string name, string email)
        {
            Id = id;
            Name = name;
            Email = email;
        }

    }
}
 