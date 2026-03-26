using System.Threading.Tasks;
using QuizzApp.DTOs;

namespace QuizzApp.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, AuthResponseDTO? Data)> RegisterAsync(RegisterDTO dto);
        Task<(bool Success, string Message, AuthResponseDTO? Data)> LoginAsync(LoginDTO dto);
        Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordDTO dto);
    }
}   