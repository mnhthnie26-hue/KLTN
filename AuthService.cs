using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QualityEvalAPI.Data;
using QualityEvalAPI.DTOs;
using QualityEvalAPI.Models;

namespace QualityEvalAPI.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<LoginResponse?> LoginAsync(string username, string password, string? ip)
    {
        var user = await _db.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == username && u.Status);

        if (user == null) return null;
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;

        var token = GenerateJwtToken(user, out var expiresAt);

        _db.LoginLogs.Add(new LoginLog { UserId = user.UserId, Action = "LOGIN", IpAddress = ip });
        await _db.SaveChangesAsync();

        return new LoginResponse(token, user.FullName, user.Role!.RoleCode, expiresAt);
    }

    public async Task LogoutAsync(int userId, string? ip)
    {
        _db.LoginLogs.Add(new LoginLog { UserId = userId, Action = "LOGOUT", IpAddress = ip });
        await _db.SaveChangesAsync();
    }

    public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword, string? ip)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return false;
        if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash)) return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        _db.LoginLogs.Add(new LoginLog { UserId = userId, Action = "CHANGE_PASSWORD", IpAddress = ip });
        await _db.SaveChangesAsync();
        return true;
    }

    public string GenerateJwtToken(User user, out DateTime expiresAt)
    {
        var jwtSection = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expireMinutes = int.Parse(jwtSection["ExpireMinutes"] ?? "120");
        expiresAt = DateTime.UtcNow.AddMinutes(expireMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role?.RoleCode ?? "STUDENT"),
            new("fullName", user.FullName)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
