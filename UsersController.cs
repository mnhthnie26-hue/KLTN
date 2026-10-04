using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QualityEvalAPI.Data;
using QualityEvalAPI.DTOs;
using QualityEvalAPI.Models;

namespace QualityEvalAPI.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "ADMIN")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    public UsersController(AppDbContext db) => _db = db;

    // GET api/users?role=STUDENT
    // Yêu cầu 1: Quản lý sinh viên, giảng viên, cán bộ quản lý
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? role)
    {
        var query = _db.Users.Include(u => u.Role).AsQueryable();
        if (!string.IsNullOrEmpty(role)) query = query.Where(u => u.Role!.RoleCode == role);

        var list = await query.Select(u => new UserResponse(
            u.UserId, u.Username, u.FullName, u.Email, u.Role!.RoleCode, u.Role.RoleName, u.Department, u.Status
        )).ToListAsync();

        return Ok(list);
    }

    // GET api/users/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var u = await _db.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.UserId == id);
        if (u == null) return NotFound();
        return Ok(new UserResponse(u.UserId, u.Username, u.FullName, u.Email, u.Role!.RoleCode, u.Role.RoleName, u.Department, u.Status));
    }

    // POST api/users
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Username == request.Username))
            return BadRequest(new { message = "Tên đăng nhập đã tồn tại." });

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleCode == request.RoleCode);
        if (role == null) return BadRequest(new { message = "Vai trò không hợp lệ." });

        var user = new User
        {
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            Email = request.Email,
            RoleId = role.RoleId,
            Department = request.Department,
            StudentCode = request.StudentCode,
            StaffCode = request.StaffCode,
            Status = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Ok(new { user.UserId, message = "Tạo người dùng thành công." });
    }

    // PUT api/users/{id}
    // Yêu cầu 2: Phân quyền truy cập theo vai trò
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleCode == request.RoleCode);
        if (role == null) return BadRequest(new { message = "Vai trò không hợp lệ." });

        user.FullName = request.FullName;
        user.Email = request.Email;
        user.Phone = request.Phone;
        user.RoleId = role.RoleId;
        user.Department = request.Department;
        user.Status = request.Status;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Cập nhật người dùng thành công." });
    }

    // PUT api/users/{id}/reset-password
    [HttpPut("{id:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] string newPassword)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Đặt lại mật khẩu thành công." });
    }

    // DELETE api/users/{id}  (thực chất khóa tài khoản để giữ toàn vẹn dữ liệu)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        user.Status = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Đã khóa tài khoản người dùng." });
    }

    // GET api/users/roles
    [HttpGet("roles")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _db.Roles.Select(r => new { r.RoleId, r.RoleCode, r.RoleName }).ToListAsync();
        return Ok(roles);
    }
}
