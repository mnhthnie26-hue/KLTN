using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QualityEvalAPI.DTOs;
using QualityEvalAPI.Services;

namespace QualityEvalAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // POST api/auth/login
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.LoginAsync(request.Username, request.Password, ip);
        if (result == null)
            return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không đúng." });

        return Ok(result);
    }

    // POST api/auth/logout
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        await _authService.LogoutAsync(userId, ip);
        return Ok(new { message = "Đăng xuất thành công." });
    }

    // POST api/auth/change-password
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var success = await _authService.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword, ip);
        if (!success) return BadRequest(new { message = "Mật khẩu cũ không đúng." });
        return Ok(new { message = "Đổi mật khẩu thành công." });
    }

    // GET api/auth/me
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        return Ok(new
        {
            username = User.Identity?.Name,
            fullName = User.FindFirstValue("fullName"),
            role = User.FindFirstValue(ClaimTypes.Role)
        });
    }
}
