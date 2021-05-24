using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;

using CartaWeb.Models.DocumentItem;
using CartaWeb.Models.Options;


namespace CartaWeb.Controllers
{
    /// <summary>
    /// Serves information about the currently authenticated user and allows for actions such as sign-in and sign-out.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        /// <summary>
        /// The Cognito options for this controller
        /// </summary>
        private readonly AwsCognitoOptions _options;

        /// <summary>
        /// The Cognito identity provider for this controller.
        /// </summary>
        private readonly IAmazonCognitoIdentityProvider _identityProvider;

        /// <summary>
        /// Creates a new instance of the <see cref="UserController"/> class with a specified controller.
        /// <param name="options">Cognito options.</param>
        /// <param name="identityProvider">A Cognito identity provider.</param>
        /// </summary>
        public UserController(IOptions<AwsCognitoOptions> options, IAmazonCognitoIdentityProvider identityProvider)
        {
            _options = options.Value;
            _identityProvider = identityProvider;
        }

        /// <summary>
        /// Helper method to return the value of a Cognito attribute type.
        /// <param name="attributes">Cognito attribute type.</param>
        /// <param name="attributeName">The name of the attribute to retrieve.</param>
        /// <returns>
        /// The attribute value.
        /// </returns>
        /// </summary>
        private string getUserAttribute(List<AttributeType> attributes, string attributeName)
        {
            AttributeType attribute = attributes.Find(i => i.Name == attributeName);
            if (attribute is not null) return attribute.Value;
            else return null;
        }

        /// <summary>
        /// Gets information about the currently authenticated user.
        /// </summary>
        /// <request name="Example"></request>
        /// <returns status="200">
        /// A dictionary of key-value pairs of user information.
        /// </returns>
        [Authorize]
        [HttpGet]
        public ActionResult<Dictionary<string, string>> GetUser()
        {
            string email = User.FindFirstValue(ClaimTypes.Email);
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string userName = User.FindFirstValue("cognito:username");

            Dictionary<string, string> info = new Dictionary<string, string>();
            info.Add("email", email);
            info.Add("username", userName);
            info.Add("id", userId);

            return Ok(info);
        }

        /// <summary>
        /// Gets a list of users in the specfied user group.
        /// </summary>
        /// <param name="groupName">The name of the user group.</param>
        /// <request name="Example">
        ///     <arg name="groupName">MyGroup</arg>
        /// </request>
        /// <response name="Example">
        ///     <body>
        ///     [
        ///         {
        ///             "id": "c07e2b1f-f1ef-4a76-989c-0b51f14f9baa",
        ///             "name": "user2",
        ///             "group": "MultipleUsersGroup"
        ///         },
        ///         {
        ///             "id": "25e48ae8-ac89-4a2b-900d-d0a746288069",
        ///             "name": "user1",
        ///            "group": "MultipleUsersGroup"
        ///         },
        ///     ]
        ///     </body>
        /// </response>
        /// <returns status="200">A list of users in the group.</returns>
        /// <returns status="404">
        /// Occurs when the specified group does not exist. 
        /// </returns>
        [Authorize]
        [HttpGet("group/{groupName}")]
        public async Task<ActionResult<List<UserItem>>> GetUsersInGroup(
            [FromRoute] string groupName
        )
        {
            List<UserItem> userItems = new() { };

            ListUsersInGroupRequest request = new ListUsersInGroupRequest();
            request.GroupName = groupName;
            request.UserPoolId = _options.UserPoolId;

            do
            {
                ListUsersInGroupResponse response;
                try
                {
                    response = await _identityProvider.ListUsersInGroupAsync(request);
                }
                catch (Amazon.CognitoIdentityProvider.Model.ResourceNotFoundException e)
                {
                    return NotFound(userItems);
                }

                foreach (UserType user in response.Users)
                {
                    UserItem userItem = new UserItem
                    (
                        getUserAttribute(user.Attributes, "sub"),
                        user.Username,
                        getUserAttribute(user.Attributes, "email")
                    );
                    userItem.Group = groupName;
                    userItems.Add(userItem);
                }

                if ((response.NextToken is not null) & (response.NextToken != ""))
                {
                    request.NextToken = response.NextToken;
                }
                else
                {
                    request.NextToken = null;
                }
            }
            while ((request.NextToken is not null) & (request.NextToken != ""));

            return Ok(userItems);
        }

        /// <summary>
        /// Attempts to sign-in a user via a challenge if not authenticated. Otherwise, does nothing.
        /// </summary>
        /// <request name="Example"></request>
        /// <returns status="200">
        /// Sets the appropriate authentication cookies on the client.
        /// </returns>
        [HttpGet("signin")]
        public IActionResult SignInUser()
        {
            if (User.Identity.IsAuthenticated)
                return SignIn(User);
            else
                return Challenge();
        }
        /// <summary>
        /// Attempts to sign-out a user if authenticated.
        /// </summary>
        /// <request name="Example"></request>
        /// <returns status="200">
        /// Removes the appropriate authentication cookies from the client.
        /// </returns>
        [HttpGet("signout")]
        public IActionResult SignOutUser()
        {
            return SignOut();
        }
    }
}