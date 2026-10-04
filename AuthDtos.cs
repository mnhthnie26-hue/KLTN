namespace QualityEvalAPI.DTOs;

public record LoginRequest(string Username, string Password);

public record LoginResponse(string Token, string FullName, string RoleCode, DateTime ExpiresAt);

public record ChangePasswordRequest(string OldPassword, string NewPassword);

public record CreateUserRequest(
    string Username,
    string Password,
    string FullName,
    string? Email,
    string RoleCode,
    string? Department,
    string? StudentCode,
    string? StaffCode
);

public record UpdateUserRequest(
    string FullName,
    string? Email,
    string? Phone,
    string RoleCode,
    string? Department,
    bool Status
);

public record UserResponse(
    int UserId,
    string Username,
    string FullName,
    string? Email,
    string RoleCode,
    string RoleName,
    string? Department,
    bool Status
);
