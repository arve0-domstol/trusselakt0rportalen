using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saksbehandling.Api.Data;

namespace Saksbehandling.Api.Controllers;

// Same credential options as AdminUsersController, for the same reason:
// both interactive admins and internal automation may need to trigger
// retention cleanup.
[ApiController]
[Route("api/admin/audit-log")]
[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme + ",BasicAuth,InternalApiKey")]
public class AdminAuditLogController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminAuditLogController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Removes old audit entries so the table doesn't grow without bound.
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> Cleanup([FromQuery] int olderThanDays = 365)
    {
        var cutoff = DateTime.UtcNow.AddDays(-olderThanDays);
        var toDelete = _db.AuditEvents.Where(e => e.Timestamp < cutoff);

        var count = await toDelete.CountAsync();
        _db.AuditEvents.RemoveRange(toDelete);
        await _db.SaveChangesAsync();

        return Ok(new { message = $"Slettet {count} historikkoppføringer." });
    }
}
