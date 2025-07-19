using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        // [FromBody] attribute'u, isteğin gövdesindeki JSON verisini bu sınıfa bağlar.
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
          
            var result = await _authService.RegisterAsync(request.Username, request.Email, request.Password);
            return Ok(result);
        }

        [HttpPost("login")]
        // [FromBody] attribute'u, isteğin gövdesindeki JSON verisini bu sınıfa bağlar.
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // AuthService'deki LoginAsync metodunu çağırırken
            // request objesinin içindeki property'leri kullanıyoruz.
            var result = await _authService.LoginAsync(request.Email, request.Password);
            return Ok(result);
        }
    }


    public class RegisterRequest
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

  
    public class LoginRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}