using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizzApp.DTOs;
using QuizzApp.Interfaces;

namespace QuizzApp.Controllers
{
    // UserController handles profile viewing and updating
    // All endpoints require a valid JWT token
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All endpoints need to be logged in
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // Helper: extract the logged-in user's Id from the JWT token claims
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int id) ? id : 0;
        }

        // GET api/user/profile
        // View the logged-in user's profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            int userId = GetUserId();
            var profile = await _userService.GetProfileAsync(userId);

            if (profile == null)
                return NotFound(ApiResponse<UserProfileDTO>.Fail("User not found."));

            return Ok(ApiResponse<UserProfileDTO>.Ok(profile));
        }

        // GET api/user/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _userService.GetUserStatsAsync(GetUserId());
            return Ok(ApiResponse<UserStatsDTO>.Ok(stats));
        }

        // PUT api/user/profile
        // Update the logged-in user's profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDTO dto)
        {
            int userId = GetUserId();
            var (success, message) = await _userService.UpdateProfileAsync(userId, dto);

            if (!success)
                return BadRequest(ApiResponse<string>.Fail(message));

            return Ok(ApiResponse<string>.Ok("Profile updated.", message));
        }
        // PUT api/user/upgrade-to-premium
        [HttpPut("upgrade-to-premium")]
        public async Task<IActionResult> UpgradeToPremium()
        {
            var (success, message, data) = await _userService.UpgradeToPremiumAsync(GetUserId());
            if (!success) return BadRequest(ApiResponse<UpgradeResponseDTO>.Fail(message));
            return Ok(ApiResponse<UpgradeResponseDTO>.Ok(data!, message));
        }
    }
}
