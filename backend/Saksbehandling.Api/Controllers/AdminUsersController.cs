using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saksbehandling.Api.Auth;
using Saksbehandling.Api.Data;
using Saksbehandling.Api.Dtos;
using Saksbehandling.Api.Models;

namespace Saksbehandling.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(AuthenticationSchemes = "CookieAdmin,BasicAuth,InternalApiKey", Roles = "Administrator")]
public class AdminUsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminUsersController(AppDbContext db)
    {
        _db = db;
    }

    // Users are referenced by a hash of their id instead of the raw id, so
    // the lookup can't be done directly in SQL and is done in memory instead.
    private async Task<User?> FindByIdHashAsync(string idHash)
    {
        var users = await _db.Users.ToListAsync();
        return users.FirstOrDefault(u => UserReferenceHasher.Hash(u.Id) == idHash);
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var users = await _db.Users
            .Include(u => u.Department)
            .OrderBy(u => u.FullName)
            .ToListAsync();

        return Ok(users.Select(UserDetailDto.From));
    }

    [HttpGet("{idHash}")]
    public async Task<IActionResult> GetById(string idHash)
    {
        var user = await FindByIdHashAsync(idHash);
        if (user is null)
        {
            return NotFound();
        }

        await _db.Entry(user).Reference(u => u.Department).LoadAsync();
        return Ok(UserDetailDto.From(user));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest request)
    {
        var user = new User
        {
            Username = request.Username,
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            Role = request.Role,
            DepartmentId = request.DepartmentId,
            PasswordHash = PasswordHasher.Hash(request.Password),
            IsActive = true,
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var created = await _db.Users.Include(u => u.Department).FirstAsync(u => u.Id == user.Id);
        return Ok(UserDetailDto.From(created));
    }

    [HttpPut("{idHash}/role")]
    public async Task<IActionResult> UpdateRole(string idHash, UpdateUserRoleRequest request)
    {
        var user = await FindByIdHashAsync(idHash);
        if (user is null)
        {
            return NotFound();
        }

        user.Role = request.Role;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Rolle oppdatert." });
    }

    [HttpPut("{idHash}/status")]
    public async Task<IActionResult> UpdateStatus(string idHash, UpdateUserStatusRequest request)
    {
        var user = await FindByIdHashAsync(idHash);
        if (user is null)
        {
            return NotFound();
        }

        user.IsActive = request.IsActive;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Status oppdatert." });
    }
}
