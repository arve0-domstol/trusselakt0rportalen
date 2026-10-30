using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saksbehandling.Api.Data;
using Saksbehandling.Api.Dtos;
using Saksbehandling.Api.Models;
using Saksbehandling.Api.Repositories;

namespace Saksbehandling.Api.Controllers;

[ApiController]
[Route("api/v2/cases")]
[Authorize]
public class CasesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly CaseRepository _repository;

    public CasesController(AppDbContext db, CaseRepository repository)
    {
        _db = db;
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] CaseListQuery query)
    {
        var parameters = new CaseSearchParameters
        {
            // With no explicit department filter and no "mine" flag, the
            // list defaults to the caller's own department.
            DepartmentId = query.Mine == true ? null : query.DepartmentId ?? GetCallerDepartmentId(),
            AssignedToUserId = query.Mine == true ? GetCallerUserId() : null,
            Status = query.Status,
            Priority = query.Priority,
            Search = query.Search,
            Sort = query.Sort,
            SortDirection = query.SortDirection,
            Page = query.Page,
            PageSize = query.PageSize,
        };

        var (items, totalCount) = await _repository.SearchAsync(parameters);

        return Ok(new PagedResult<CaseSummaryDto>
        {
            Items = items.Select(CaseSummaryDto.From).ToList(),
            TotalCount = totalCount,
            Page = parameters.Page < 1 ? 1 : parameters.Page,
            PageSize = parameters.PageSize is < 1 or > 100 ? 20 : parameters.PageSize,
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var found = await _db.Cases
            .Include(c => c.Customer)
            .Include(c => c.Department)
            .Include(c => c.AssignedToUser)
            .Include(c => c.ApprovedByUser)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (found is null)
        {
            return NotFound();
        }

        return Ok(CaseDetailDto.From(found));
    }

    private async Task<Case> LoadDetailAsync(int id) => await _db.Cases
        .Include(c => c.Customer)
        .Include(c => c.Department)
        .Include(c => c.AssignedToUser)
        .Include(c => c.ApprovedByUser)
        .FirstAsync(c => c.Id == id);

    [HttpPost]
    public async Task<IActionResult> Create(CreateCaseRequest request)
    {
        var nextSeq = await _db.Cases.CountAsync() + 1;

        var newCase = new Case
        {
            CaseNumber = $"SAK-{DateTime.UtcNow:yyyy}-{nextSeq:D5}",
            Title = request.Title,
            Description = request.Description,
            CustomerId = request.CustomerId,
            DepartmentId = request.DepartmentId ?? GetCallerDepartmentId(),
            Priority = request.Priority,
            Status = CaseStatus.Ny,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Deadline = request.Deadline,
        };

        _db.Cases.Add(newCase);
        await _db.SaveChangesAsync();

        _db.AuditEvents.Add(new AuditEvent
        {
            EntityType = nameof(Case),
            EntityId = newCase.Id,
            UserId = GetCallerUserId(),
            Action = "Opprettet",
            Details = $"Sak {newCase.CaseNumber} opprettet.",
        });
        await _db.SaveChangesAsync();

        var created = await _db.Cases
            .Include(c => c.Customer)
            .Include(c => c.Department)
            .Include(c => c.AssignedToUser)
            .FirstAsync(c => c.Id == newCase.Id);

        return CreatedAtAction(nameof(GetById), new { id = newCase.Id }, CaseDetailDto.From(created));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateCaseRequest request)
    {
        var existing = await _db.Cases.FindAsync(id);
        if (existing is null)
        {
            return NotFound();
        }

        existing.Title = request.Title;
        existing.Description = request.Description;
        existing.Status = request.Status;
        existing.Priority = request.Priority;
        existing.AssignedToUserId = request.AssignedToUserId;
        existing.DepartmentId = request.DepartmentId;
        existing.Deadline = request.Deadline;
        existing.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        _db.AuditEvents.Add(new AuditEvent
        {
            EntityType = nameof(Case),
            EntityId = existing.Id,
            UserId = GetCallerUserId(),
            Action = "Oppdatert",
            Details = $"Sak {existing.CaseNumber} oppdatert.",
        });
        await _db.SaveChangesAsync();

        var updated = await _db.Cases
            .Include(c => c.Customer)
            .Include(c => c.Department)
            .Include(c => c.AssignedToUser)
            .FirstAsync(c => c.Id == existing.Id);

        return Ok(CaseDetailDto.From(updated));
    }

    [HttpPost("{id:int}/send-reminder")]
    public async Task<IActionResult> SendReminder(int id)
    {
        var found = await _db.Cases.FindAsync(id);
        if (found is null)
        {
            return NotFound();
        }

        if (found.AssignedToUserId is null)
        {
            return BadRequest(new { message = "Saken er ikke tildelt noen ennå." });
        }

        _db.Notifications.Add(new Notification
        {
            UserId = found.AssignedToUserId.Value,
            Message = $"Påminnelse: Sak {found.CaseNumber} venter på oppfølging.",
            RelatedCaseId = found.Id,
        });
        await _db.SaveChangesAsync();

        return Ok(new { message = "Påminnelse sendt." });
    }

    [HttpPost("{id:int}/assign")]
    public async Task<IActionResult> Assign(int id, AssignCaseRequest request)
    {
        var found = await _db.Cases.FindAsync(id);
        if (found is null)
        {
            return NotFound();
        }

        found.AssignedToUserId = request.AssignedToUserId;
        found.UpdatedAt = DateTime.UtcNow;

        _db.CaseAssignments.Add(new CaseAssignment
        {
            CaseId = id,
            AssignedToUserId = request.AssignedToUserId,
            AssignedByUserId = GetCallerUserId(),
            Note = "Tildelt på nytt.",
        });

        _db.AuditEvents.Add(new AuditEvent
        {
            EntityType = nameof(Case),
            EntityId = id,
            UserId = GetCallerUserId(),
            Action = "Tildelt",
            Details = $"Sak tildelt bruker {request.AssignedToUserId}.",
        });

        await _db.SaveChangesAsync();

        return Ok(CaseDetailDto.From(await LoadDetailAsync(id)));
    }

    /// <summary>Approves a case.</summary>
    [HttpGet("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var found = await _db.Cases.FindAsync(id);
        if (found is null)
        {
            return NotFound();
        }

        found.Status = CaseStatus.Godkjent;
        found.ApprovedByUserId = GetCallerUserId();
        found.UpdatedAt = DateTime.UtcNow;

        _db.AuditEvents.Add(new AuditEvent
        {
            EntityType = nameof(Case),
            EntityId = id,
            UserId = GetCallerUserId(),
            Action = "Godkjent",
            Details = "Sak godkjent.",
        });

        await _db.SaveChangesAsync();

        return Ok(CaseDetailDto.From(await LoadDetailAsync(id)));
    }

    [HttpPost("{id:int}/close")]
    public async Task<IActionResult> Close(int id, CloseCaseRequest request)
    {
        var found = await _db.Cases.FindAsync(id);
        if (found is null)
        {
            return NotFound();
        }

        found.Status = CaseStatus.Lukket;
        found.ClosedAt = DateTime.UtcNow;
        found.UpdatedAt = DateTime.UtcNow;

        _db.AuditEvents.Add(new AuditEvent
        {
            EntityType = nameof(Case),
            EntityId = id,
            UserId = GetCallerUserId(),
            Action = "Lukket",
            Details = string.IsNullOrWhiteSpace(request.DecisionText)
                ? "Sak lukket."
                : $"Sak lukket. Vedtak: {request.DecisionText}",
        });

        await _db.SaveChangesAsync();

        return Ok(CaseDetailDto.From(await LoadDetailAsync(id)));
    }

    private int GetCallerUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private int GetCallerDepartmentId() => int.Parse(User.FindFirstValue("departmentId")!);
}
