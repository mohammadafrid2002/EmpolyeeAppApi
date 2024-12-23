using Microsoft.AspNetCore.Mvc;

namespace EmpAppApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // Validate credentials (this is just an example, replace with your logic)
            if (request.Email == "admin@gmail.com" && request.Password == "123")
            {
                return Ok(new { message = "Login successful!" });
            }
            return Unauthorized(new { message = "Invalid credentials." });
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
