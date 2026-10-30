using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saksbehandling.Api.Data;
using Saksbehandling.Api.Dtos;

namespace Saksbehandling.Api.Controllers;

[ApiController]
[Route("api/v2/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _db;

    public NotificationsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var userId = GetCallerUserId();
        var notifications = await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync();

        return Ok(notifications.Select(NotificationDto.From));
    }

    [HttpPost("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var userId = GetCallerUserId();
        var notification = await _db.Notifications.FindAsync(id);
        if (notification is null || notification.UserId != userId)
        {
            return NotFound();
        }

        notification.IsRead = true;
        await _db.SaveChangesAsync();
        return Ok();
    }

    private int GetCallerUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
