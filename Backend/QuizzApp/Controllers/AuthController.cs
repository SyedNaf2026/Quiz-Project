using Microsoft.AspNetCore.Mvc;
using QuizzApp.DTOs;
using QuizzApp.Interfaces;
using QuizzApp.Services;

namespace QuizzApp.Controllers
{
    // AuthController handles user registration and login
    // No [Authorize] here - these are public endpoints
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST api/auth/register
        // Register a new user
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
            var (success, message, data) = await _authService.RegisterAsync(dto);

            if (!success)
                return BadRequest(ApiResponse<string>.Fail(message));

            return Ok(ApiResponse<AuthResponseDTO>.Ok(data!, message));
        }

        // POST api/auth/login
        // Login with email and password, returns JWT token
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            var (success, message, data) = await _authService.LoginAsync(dto);

            if (!success)
                return Unauthorized(ApiResponse<AuthResponseDTO>.Fail(message));

            return Ok(ApiResponse<AuthResponseDTO>.Ok(data!, message));
        }

        // POST api/auth/reset-password
        // Reset password by email (no email server needed - direct reset)
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
        {
            var (success, message) = await _authService.ResetPasswordAsync(dto);
            if (!success)
                return BadRequest(ApiResponse<string>.Fail(message));
            return Ok(ApiResponse<string>.Ok("Password reset successfully.", message));
        }
    }
}
