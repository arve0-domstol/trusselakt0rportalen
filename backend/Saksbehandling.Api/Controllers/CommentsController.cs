using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saksbehandling.Api.Data;
using Saksbehandling.Api.Dtos;
using Saksbehandling.Api.Models;

namespace Saksbehandling.Api.Controllers;

[ApiController]
[Route("api/v2/cases/{caseId:int}/comments")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public CommentsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> List(int caseId)
    {
        var comments = await _db.Comments
            .Include(c => c.AuthorUser)
            .Where(c => c.CaseId == caseId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return Ok(comments.Select(CommentDto.From));
    }

    [HttpPost]
    public async Task<IActionResult> Create(int caseId, CreateCommentRequest request)
    {
        var caseExists = await _db.Cases.AnyAsync(c => c.Id == caseId);
        if (!caseExists)
        {
            return NotFound();
        }

        var comment = new Comment
        {
            CaseId = caseId,
            AuthorUserId = GetCallerUserId(),
            Text = request.Text,
            IsInternal = request.IsInternal,
        };
        _db.Comments.Add(comment);

        _db.AuditEvents.Add(new AuditEvent
        {
            EntityType = nameof(Case),
            EntityId = caseId,
            UserId = GetCallerUserId(),
            Action = "KommentarLagtTil",
            Details = request.IsInternal ? "Intern kommentar lagt til." : "Kommentar lagt til.",
        });

        await _db.SaveChangesAsync();

        var created = await _db.Comments.Include(c => c.AuthorUser).FirstAsync(c => c.Id == comment.Id);
        return Ok(CommentDto.From(created));
    }

    private int GetCallerUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
