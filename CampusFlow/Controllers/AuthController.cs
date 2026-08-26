using CampusFlow.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusFlow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthCookieService _authCookieService;

        public AuthController(IAuthCookieService authCookieService)
        {
            _authCookieService = authCookieService;
        }

        // POST /api/auth/logout
        // Clears the JWT cookie so the user is logged out.
        // The frontend calls this when the user clicks "Sign Out".
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            _authCookieService.ClearAuthCookie(Response);
            return Ok(new { message = "Logged out successfully." });
        }
    }
}
