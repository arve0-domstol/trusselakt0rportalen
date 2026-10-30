using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saksbehandling.Api.Data;
using Saksbehandling.Api.Dtos;
using Saksbehandling.Api.Models;

namespace Saksbehandling.Api.Controllers;

[ApiController]
[Route("api/v2/cases/{caseId:int}/history")]
[Authorize]
public class CaseHistoryController : ControllerBase
{
    private readonly AppDbContext _db;

    public CaseHistoryController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> List(int caseId)
    {
        var events = await _db.AuditEvents
            .Include(e => e.User)
            .Where(e => e.EntityType == nameof(Case) && e.EntityId == caseId)
            .OrderByDescending(e => e.Timestamp)
            .ToListAsync();

        return Ok(events.Select(AuditEventDto.From));
    }
}
