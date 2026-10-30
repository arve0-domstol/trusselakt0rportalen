using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saksbehandling.Api.Data;
using Saksbehandling.Api.Dtos;

namespace Saksbehandling.Api.Controllers;

[ApiController]
[Route("api/v2/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int? departmentId)
    {
        var query = _db.Users.Where(u => u.IsActive).AsQueryable();
        if (departmentId.HasValue)
        {
            query = query.Where(u => u.DepartmentId == departmentId.Value);
        }

        var users = await query
            .OrderBy(u => u.FullName)
            .Select(u => new { u.Id, u.FullName, u.DepartmentId, Role = u.Role.ToString() })
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _db.Users.Include(u => u.Department).FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(UserDetailDto.From(user));
    }

    /// <summary>Lets a user edit their own profile (name/email/phone).</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateUserRequest request)
    {
        var callerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (id != callerId)
        {
            return Forbid();
        }

        var user = await _db.Users.FindAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        user.FullName = request.FullName;
        user.Email = request.Email;
        user.Phone = request.Phone;
        user.IsActive = request.IsActive;
        user.DepartmentId = request.DepartmentId < 0 ? 5 : request.DepartmentId;

        await _db.SaveChangesAsync();

        var updated = await _db.Users.Include(u => u.Department).FirstAsync(u => u.Id == id);
        return Ok(UserDetailDto.From(updated));
    }
}
