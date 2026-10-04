namespace QualityEvalAPI.Models;

public class Role
{
    public int RoleId { get; set; }
    public string RoleCode { get; set; } = null!;   // ADMIN, MANAGER, LECTURER, STUDENT
    public string RoleName { get; set; } = null!;
    public string? Description { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
}

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int RoleId { get; set; }
    public Role? Role { get; set; }
    public string? Department { get; set; }
    public string? StudentCode { get; set; }
    public string? StaffCode { get; set; }
    public bool Status { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class LoginLog
{
    public int LogId { get; set; }
    public int UserId { get; set; }
    public string Action { get; set; } = null!;  // LOGIN, LOGOUT, CHANGE_PASSWORD
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
