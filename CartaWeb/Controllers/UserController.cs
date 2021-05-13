using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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