using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;

using CartaWeb.Models.Data;
using CartaWeb.Models.Options;

using Microsoft.Extensions.Logging;

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
        /// The logger for this controller.
        /// </summary>
        private readonly ILogger<UserController> _logger;

        /// <summary>
        /// The Cognito options for this controller
        /// </summary>
        private readonly AwsCdkOptions _options;

        /// <summary>
        /// The Cognito identity provider for this controller.
        /// </summary>
        private readonly IAmazonCognitoIdentityProvider _identityProvider;

        /// <summary>
        /// Dictionary that maps user attributes to Cognito user attrubute names
        /// </summary>
        private static readonly Dictionary<string, string> _attributeDictionary =
        new()
        {
            { "UserId", "sub" },
            { "UserName", "username" },
            { "Email", "email" },
            { "FirstName", "given_name" },
            { "LastName", "family_name" }
        };

        /// <summary>
        /// Creates a new instance of the <see cref="UserController"/> class with a specified controller.
        /// <param name="options">Cognito options.</param>
        /// <param name="identityProvider">A Cognito identity provider.</param>
        /// <param name="logger">The logger for the controller.</param>
        /// </summary>
        public UserController(
            IOptions<AwsCdkOptions> options,
            IAmazonCognitoIdentityProvider identityProvider,
            ILogger<UserController> logger)
        {
            _options = options.Value;
            _identityProvider = identityProvider;
            _logger = logger;
        }

        /// <summary>
        /// Helper method to return the value of a Cognito attribute type.
        /// <param name="attributes">Cognito attribute type.</param>
        /// <param name="attributeName">The name of the attribute to retrieve.</param>
        /// <returns>
        /// The attribute value.
        /// </returns>
        /// </summary>
        private static string GetUserAttribute(List<AttributeType> attributes, string attributeName)
        {
            AttributeType attribute = attributes.Find(i => i.Name == attributeName);
            if (attribute is not null) return attribute.Value;
            else return null;
        }

        /// <summary>
        /// Helper method to populuate and return user information for the currently logged in user.
        /// </summary>
        /// <param name="user">The user claims principal.</param>
        /// <returns>
        /// The user information.
        /// </returns>
        public static UserInformation GetUserInformation(ClaimsPrincipal user)
        {
            UserInformation userInformation = new
            (
                user.FindFirstValue(ClaimTypes.NameIdentifier),
                user.FindFirstValue("cognito:username")
            )
            {
                Email = user.FindFirstValue(ClaimTypes.Email),
                FirstName = user.FindFirstValue(ClaimTypes.GivenName),
                LastName = user.FindFirstValue(ClaimTypes.Surname),
                Groups = new List<string>()
            };
            foreach (Claim claim in user.FindAll("cognito:groups"))
            {
                if (!userInformation.Groups.Contains(claim.Value)) userInformation.Groups.Add(claim.Value);
            }
            return userInformation;
        }

        /// <summary>
        /// Determines whether the user is currently authenticated or not.
        /// </summary>
        /// <returns status="200">A boolean indicated whether the user is authenticated.</returns>
        [HttpGet("authenticated")]
        public ActionResult<bool> IsUserAuthenticated()
        {
            // TODO: (Permissions) This endpoint should be available to all users.

            return Ok(User.Identity.IsAuthenticated);
        }

        /// <summary>
        /// Gets information about the currently authenticated user.
        /// </summary>
        /// <request name="Example"></request>
        /// <returns status="200">
        /// User information will be attached to the response body.
        /// </returns>
        [Authorize]
        [HttpGet]
        public ActionResult<UserInformation> GetUser()
        {
            // TODO: (Permissions) This endpoint should be available to all users.

            return Ok(GetUserInformation(User));
        }

        /// <summary>
        /// Get a list of users according to filter criteria.
        /// </summary>
        /// <param name="attributeName">The name of the attribute to filter on.
        /// Set to null if all users should be returned.</param>
        /// <param name="attributeValue">The value of the attribute to filter on.</param>
        /// <param name="attributeFilter">Filter criteria: "=" denotes exact match, "^=" denotes begins with.</param>
        /// <request name="Example all users">
        ///     <arg name="attributeName">UserName</arg>
        ///     <arg name="attributeValue">Ma</arg>
        ///     <arg name="attributeFilter">^=</arg>
        /// </request>
        /// <request name="Example exact match on email">
        ///     <arg name="attributeName">Email</arg>
        ///     <arg name="attributeValue">myemail@email.com</arg>
        ///     <arg name="attributeFilter">=</arg>
        /// </request>
        /// <request name="Example username starts with">
        ///     <arg name="attributeName">UserName</arg>
        ///     <arg name="attributeValue">Ma</arg>
        ///     <arg name="attributeFilter">^=</arg>
        /// </request>
        /// <returns status="200">A list of users is attached to the response body.</returns>
        /// <returns status="400">Occurs when the request is invalid.</returns>
        [Authorize]
        [HttpGet("users")]
        public async Task<ActionResult<List<UserInformation>>> GetUsers(
            UserAttributeEnumeration? attributeName,
            string attributeValue,
            string attributeFilter)
        {
            // TODO: (Permissions) This endpoint should be available to all users.
            // -     For now, we can keep it open to all users or restrict it to users within organizations groups.
            // -     However, in the future, it should be modified to reflect the visability of users based on the
            //       permissions system. For instance, we may add some permissions flags between a source groups and a
            //       target groups to indicate whether the name, email, and other information of the target group should
            //       be visible to the source group. Thus, to make a user "public" to all searches, the "all" group
            //       would have a connection to the "user" group with all visibility flags set to true. 

            List<UserInformation> userInformationList = new() { };

            ListUsersRequest request = new()
            {
                UserPoolId = _options.UserPoolId
            };
            if (attributeName.HasValue)
                request.Filter = _attributeDictionary[attributeName.ToString()] +
                    attributeFilter + "\"" + attributeValue + "\"";

            do
            {
                ListUsersResponse response;
                try
                {
                    response = await _identityProvider.ListUsersAsync(request);
                }
                catch (AmazonCognitoIdentityProviderException e)
                {
                    _logger.LogError(e, "ListUsers request failed unexpectedly.");
                    return BadRequest();
                }

                foreach (UserType user in response.Users)
                {
                    UserInformation userInformation = new
                    (
                        GetUserAttribute(user.Attributes, "sub"),
                        user.Username
                    )
                    {
                        Email = GetUserAttribute(user.Attributes, "email"),
                        FirstName = GetUserAttribute(user.Attributes, "given_name"),
                        LastName = GetUserAttribute(user.Attributes, "family_name")
                    };
                    userInformationList.Add(userInformation);
                }

                if ((response.PaginationToken is not null) & (response.PaginationToken != ""))
                {
                    request.PaginationToken = response.PaginationToken;
                }
                else
                {
                    request.PaginationToken = null;
                }
            }
            while ((request.PaginationToken is not null) & (request.PaginationToken != ""));

            return userInformationList;
        }

        /// <summary>
        /// Gets a list of users in the specfied user group.
        /// </summary>
        /// <param name="groupName">The name of the user group.</param>
        /// <request name="Example">
        ///     <arg name="groupName">MyGroup</arg>
        /// </request>
        /// <returns status="200">A list of users in the group is attached to the response body.</returns>
        /// <returns status="400">Occurs when the request is invalid.</returns>
        [Authorize]
        [HttpGet("group/{groupName}")]
        public async Task<ActionResult<List<UserInformation>>> GetUsersInGroup(
            [FromRoute] string groupName
        )
        {
            // TODO: (Permissions) This endpoint should be restricted to users that are actually part of a group or have
            //       the "admin" permission over the group. Additionally, the method should be updated to reflect the
            //       addition of permissions groups whereas the current method works with Cognito groups as if they are
            //       organizations.

            List<UserInformation> userInformationList = new() { };
            ListUsersInGroupRequest request = new()
            {
                GroupName = groupName,
                UserPoolId = _options.UserPoolId
            };

            do
            {
                ListUsersInGroupResponse response;
                try
                {
                    response = await _identityProvider.ListUsersInGroupAsync(request);
                }
                catch (ResourceNotFoundException e)
                {
                    _logger.LogError(e, "ListUsersInGroup request fail because the user pool ID was not found.");
                    return NotFound();
                }
                catch (AmazonCognitoIdentityProviderException e)
                {
                    _logger.LogError(e, "ListUsersInGroup request failed unexpectedly.");
                    return BadRequest();
                }

                if ((response is null) | (response.Users is null))
                    return NotFound();

                foreach (UserType user in response.Users)
                {
                    UserInformation userInformation = new
                    (
                        GetUserAttribute(user.Attributes, "sub"),
                        user.Username
                    )
                    {
                        Email = GetUserAttribute(user.Attributes, "email"),
                        FirstName = GetUserAttribute(user.Attributes, "given_name"),
                        LastName = GetUserAttribute(user.Attributes, "family_name")
                    };
                    userInformationList.Add(userInformation);
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

            return Ok(userInformationList);
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
            // TODO: (Permissions) This endpoint should be available to all users.

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
            // TODO: (Permissions) This endpoint should be available to all users.

            return SignOut();
        }
    }
}
