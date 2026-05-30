using Microsoft.AspNetCore.Mvc;

namespace CampusFlow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        // POST /api/auth/logout
        // Clears the JWT cookie so the user is logged out.
        // The frontend calls this when the user clicks "Sign Out".
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Delete the cookie by setting its expiry to the past.
            Response.Cookies.Delete("jwt");
            return Ok(new { message = "Logged out successfully." });
        }
    }
}
