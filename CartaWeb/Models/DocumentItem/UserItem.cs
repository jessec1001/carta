using System;

namespace CartaWeb.Models.DocumentItem
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
        /// Timestamp of the date the user was added
        /// </summary>
        public DateTime DateAdded { get; set; }
        /// <summary>
        /// Timestamp of the date the user was deleted
        /// </summary>
        public DateTime? DateDeleted { get; set; }
        /// <summary>
        /// ID of user that added a user
        /// </summary>
        public string AddedBy { get; set; }
        /// <summary>
        /// ID of user that deleted a user
        /// </summary>
        public string DeletedBy { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="UserItem"/> class
        /// </summary>
        public UserItem (string id, string name)
        {
            Id = id;
            Name = name;
            DateAdded = DateTime.Now;
        }

    }
}
 