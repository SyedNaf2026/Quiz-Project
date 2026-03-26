using QuizzApp.DTOs;

namespace QuizzApp.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDTO?> GetProfileAsync(int userId);
        Task<(bool Success, string Message)> UpdateProfileAsync(int userId, UpdateProfileDTO dto);
        Task<UserStatsDTO> GetUserStatsAsync(int userId);
    }
}