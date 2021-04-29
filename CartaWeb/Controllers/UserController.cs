using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CartaWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
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

        [HttpGet("signin")]
        public IActionResult SignInUser()
        {
            if (User.Identity.IsAuthenticated)
                return SignIn(User);
            else
                return Challenge();
        }
        [HttpGet("signout")]
        public IActionResult SignOutUser()
        {
            return SignOut();
        }
    }
}