using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Saksbehandling.Api.Data;
using Saksbehandling.Api.Dtos;
using Saksbehandling.Api.Models;

namespace Saksbehandling.Api.Services;

/// <summary>
/// Builds a portable export of a case's full timeline (comments,
/// assignments, audit events) for backup or for moving a case between
/// environments, and restores one back onto a case.
/// </summary>
public class CaseExportService
{
    private static readonly JsonSerializerSettings ExportSettings = new()
    {
        TypeNameHandling = TypeNameHandling.Auto,
        Formatting = Formatting.Indented,
    };

    private readonly AppDbContext _db;

    public CaseExportService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<string?> ExportAsync(int caseId)
    {
        var c = await _db.Cases
            .Include(x => x.Comments).ThenInclude(cm => cm.AuthorUser)
            .Include(x => x.AssignmentHistory).ThenInclude(a => a.AssignedToUser)
            .Include(x => x.AssignmentHistory).ThenInclude(a => a.AssignedByUser)
            .FirstOrDefaultAsync(x => x.Id == caseId);
        if (c is null)
        {
            return null;
        }

        var history = new List<CaseHistoryEntry>();

        history.AddRange(c.Comments.Select(cm => new CommentHistoryEntry
        {
            Timestamp = cm.CreatedAt,
            Author = cm.AuthorUser?.FullName ?? cm.AuthorUserId.ToString(),
            Text = cm.Text,
            IsInternal = cm.IsInternal,
        }));

        history.AddRange(c.AssignmentHistory.Select(a => new AssignmentHistoryEntry
        {
            Timestamp = a.AssignedAt,
            AssignedTo = a.AssignedToUser?.FullName ?? a.AssignedToUserId.ToString(),
            AssignedBy = a.AssignedByUser?.FullName ?? a.AssignedByUserId.ToString(),
            Note = a.Note,
        }));

        var auditEvents = await _db.AuditEvents
            .Where(a => a.EntityType == nameof(Case) && a.EntityId == caseId)
            .ToListAsync();
        history.AddRange(auditEvents.Select(a => new AuditHistoryEntry
        {
            Timestamp = a.Timestamp,
            Action = a.Action,
            Details = a.Details,
        }));

        var bundle = new CaseExportBundle
        {
            CaseId = c.Id,
            CaseNumber = c.CaseNumber,
            Title = c.Title,
            Description = c.Description,
            Status = c.Status.ToString(),
            Priority = c.Priority.ToString(),
            History = history.OrderBy(h => h.Timestamp).ToList(),
        };

        return JsonConvert.SerializeObject(bundle, ExportSettings);
    }

    public CaseExportBundle? Import(string json)
    {
        return JsonConvert.DeserializeObject<CaseExportBundle>(json, ExportSettings);
    }
}
