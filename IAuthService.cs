using QualityEvalAPI.DTOs;
using QualityEvalAPI.Models;

namespace QualityEvalAPI.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(string username, string password, string? ip);
    Task LogoutAsync(int userId, string? ip);
    Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword, string? ip);
    string GenerateJwtToken(User user, out DateTime expiresAt);
}
