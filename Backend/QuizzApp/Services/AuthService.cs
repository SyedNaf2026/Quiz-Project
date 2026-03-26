using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using QuizzApp.DTOs;
using QuizzApp.Models;
using QuizzApp.Interfaces;

namespace QuizzApp.Services
{
    // AuthService handles user registration, login, and JWT token creation
    public class AuthService : IAuthService
    {
        private readonly IGenericRepository<User> _userRepo;
        private readonly IConfiguration _configuration;

        public AuthService(IGenericRepository<User> userRepo, IConfiguration configuration)
        {
            _userRepo = userRepo;
            _configuration = configuration;
        }

        public async Task<(bool Success, string Message, AuthResponseDTO? Data)> RegisterAsync(RegisterDTO dto)
        {
            if (dto.Role != "QuizTaker" && dto.Role != "QuizCreator")
                return (false, "Role must be 'QuizTaker' or 'QuizCreator'.",null);

            // Check if email is already taken
            var existingUsers = await _userRepo.FindAsync(u => u.Email == dto.Email);
            if (existingUsers.Any())
                return (false, "Email is already registered.",null);

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = hashedPassword,
                Role = dto.Role,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepo.AddAsync(user);

            var token = GenerateJwtToken(user);

            var response = new AuthResponseDTO
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
            };
            return (true, "Registration successful.",response);
        }

        public async Task<(bool Success, string Message, AuthResponseDTO? Data)> LoginAsync(LoginDTO dto)
        {
            var users = await _userRepo.FindAsync(u => u.Email == dto.Email);
            var user = users.FirstOrDefault();

            if (user == null)
                return (false, "Invalid email or password.", null);

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!isPasswordValid)
                return (false, "Invalid email or password.", null);

            // Generate JWT token
            var token = GenerateJwtToken(user);

            var response = new AuthResponseDTO
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            };

            return (true, "Login successful.", response);
        }

        public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordDTO dto)
        {
            var users = await _userRepo.FindAsync(u => u.Email == dto.Email);
            var user = users.FirstOrDefault();

            if (user == null)
                return (false, "No account found with that email.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _userRepo.UpdateAsync(user);
            return (true, "Password reset successfully.");
        }

        // Creates a JWT token with user claims (Id, Email, Role)
        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]!;
            var issuer = jwtSettings["Issuer"]!;
            var audience = jwtSettings["Audience"]!;
            var expiryDays = int.Parse(jwtSettings["ExpiryInDays"]!);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            // Build the token
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(expiryDays),
                signingCredentials: credentials
            );

            // Convert to string format
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
