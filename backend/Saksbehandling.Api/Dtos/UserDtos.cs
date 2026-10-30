using Saksbehandling.Api.Auth;
using Saksbehandling.Api.Models;

namespace Saksbehandling.Api.Dtos;

public class UserDetailDto
{
    public int Id { get; set; }
    public string IdHash { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public static UserDetailDto From(User u) => new()
    {
        Id = u.Id,
        IdHash = UserReferenceHasher.Hash(u.Id),
        Username = u.Username,
        FullName = u.FullName,
        Email = u.Email,
        Phone = u.Phone,
        Role = u.Role.ToString(),
        DepartmentId = u.DepartmentId,
        DepartmentName = u.Department?.Name ?? string.Empty,
        IsActive = u.IsActive,
        CreatedAt = u.CreatedAt,
        LastLoginAt = u.LastLoginAt,
    };
}

public class UpdateUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public int DepartmentId { get; set; }
    public string Password { get; set; } = string.Empty;
}

public class UpdateUserRoleRequest
{
    public UserRole Role { get; set; }
}

public class UpdateUserStatusRequest
{
    public bool IsActive { get; set; }
}
