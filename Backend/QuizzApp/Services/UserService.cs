using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using QuizzApp.Context;
using QuizzApp.DTOs;
using QuizzApp.Interfaces;
using QuizzApp.Models;

namespace QuizzApp.Services
{
    public class UserService : IUserService
    {
        private readonly IGenericRepository<User> _userRepo;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public UserService(IGenericRepository<User> userRepo, AppDbContext context, IConfiguration configuration)
        {
            _userRepo = userRepo;
            _context = context;
            _configuration = configuration;
        }

        public async Task<UserProfileDTO?> GetProfileAsync(int userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return null;

            return new UserProfileDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
            };
        }

        public async Task<(bool Success, string Message)> UpdateProfileAsync(int userId, UpdateProfileDTO dto)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                return (false, "User not found.");

            if (user.Email != dto.Email)
            {
                var existing = await _userRepo.FindAsync(u => u.Email == dto.Email && u.Id != userId);
                if (existing.Any())
                    return (false, "Email is already in use by another account.");
            }

            user.FullName = dto.FullName;
            user.Email = dto.Email;

            await _userRepo.UpdateAsync(user);
            return (true, "Profile updated successfully.");
        }

        public async Task<UserStatsDTO> GetUserStatsAsync(int userId)
        {
            var results = await _context.QuizResults
                .Include(r => r.Quiz).ThenInclude(q => q!.Category)
                .Where(r => r.UserId == userId)
                .ToListAsync();

            if (!results.Any())
                return new UserStatsDTO();

            var totalAttempts = results.Count;
            var avgScore = Math.Round(results.Average(r => r.Percentage), 1);
            var bestScore = results.Max(r => r.Percentage);
            var uniqueQuizzes = results.Select(r => r.QuizId).Distinct().Count();

            // Best category = category with highest average percentage
            var bestCategory = results
                .Where(r => r.Quiz?.Category != null)
                .GroupBy(r => r.Quiz!.Category!.Name)
                .OrderByDescending(g => g.Average(r => r.Percentage))
                .FirstOrDefault()?.Key ?? "N/A";

            return new UserStatsDTO
            {
                TotalAttempts = totalAttempts,
                AverageScore = avgScore,
                BestScore = bestScore,
                TotalQuizzesTaken = uniqueQuizzes,
                BestCategory = bestCategory
            };
        }

        public async Task<(bool Success, string Message, UpgradeResponseDTO? Data)> UpgradeToPremiumAsync(int userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                return (false, "User not found.", null);

            if (user.Role == "PremiumTaker")
                return (false, "You are already a Premium member.", null);

            if (user.Role != "QuizTaker")
                return (false, "Only QuizTakers can upgrade to Premium.", null);

            user.Role = "PremiumTaker";
            await _userRepo.UpdateAsync(user);

            // Generate fresh JWT with updated role
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]!;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiryDays = int.Parse(jwtSettings["ExpiryInDays"]!);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(expiryDays),
                signingCredentials: credentials
            );

            return (true, "Upgraded to Premium successfully!", new UpgradeResponseDTO
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Role = "PremiumTaker",
                Message = "Welcome to Premium! You now have unlimited quiz attempts."
            });
        }
    }
}
