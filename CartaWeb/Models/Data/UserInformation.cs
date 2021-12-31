using System.Collections.Generic;
using System.Security.Claims;

namespace CartaWeb.Models.Data
{
    /// <summary>
    /// Encapsulates user information
    /// </summary>
    public class UserInformation
    {
        /// <summary>
        /// The unique identifier for the user.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The alias for the user.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The email address for the user.
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// The first name for the user.
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// The last name for the user.
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// The list of groups that a user belongs to.
        /// </summary>
        public List<string> Groups { get; set; }


        /// <summary>
        /// Creates a new instance of the <see cref="UserInformation"/> class.
        /// </summary>
        public UserInformation() {}

        /// <summary>
        /// Creates a new instance of the <see cref="UserInformation"/> class.
        /// </summary>
        public UserInformation(string id, string name)
        {
            Id = id;
            Name = name;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="UserInformation"/> class.
        /// </summary>
        public UserInformation(ClaimsPrincipal user)
        {
            // TODO: Abstract the cognito username claim.
            Id = user.FindFirstValue(ClaimTypes.NameIdentifier);
            Name = user.FindFirstValue("cognito:username");
        }
    }
}
