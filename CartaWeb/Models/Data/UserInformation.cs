using System.Collections.Generic;

namespace CartaWeb.Models.Data
{
    /// <summary>
    /// Encapsulates user information
    /// </summary>
    public class UserInformation
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
        /// The user's given (first) name
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// The user's last name
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// List of groups that a user belongs to
        /// </summary>
        public List<string> Groups { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="UserInformation"/> class
        /// </summary>
        public UserInformation(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
