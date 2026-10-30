using Saksbehandling.Api.Models;

namespace Saksbehandling.Api.Dtos;

/// <summary>
/// The logged-in user's own profile, returned on login and from
/// <c>GET /api/me</c>. The frontend caches a copy of this in localStorage
/// so it doesn't have to re-fetch it on every page load.
/// </summary>
public class UserProfileDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;

    public static UserProfileDto From(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        FullName = user.FullName,
        Email = user.Email,
        Role = user.Role.ToString(),
        DepartmentId = user.DepartmentId,
        DepartmentName = user.Department?.Name ?? string.Empty,
    };
}
