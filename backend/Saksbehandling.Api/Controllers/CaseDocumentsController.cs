using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saksbehandling.Api.Data;
using Saksbehandling.Api.Dtos;
using Saksbehandling.Api.Models;
using Saksbehandling.Api.Services;

namespace Saksbehandling.Api.Controllers;

[ApiController]
[Route("api/v2/cases/{caseId:int}/documents")]
[Authorize]
public class CaseDocumentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly DocumentImportService _importService;

    public CaseDocumentsController(
        AppDbContext db,
        DocumentImportService importService)
    {
        _db = db;
        _importService = importService;
    }

    [HttpGet]
    public async Task<IActionResult> List(int caseId)
    {
        var documents = await _db.Documents
            .Include(d => d.UploadedByUser)
            .Where(d => d.CaseId == caseId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();

        return Ok(documents.Select(DocumentDto.From));
    }

    [HttpPost("import-url")]
    public async Task<IActionResult> ImportFromUrl(int caseId, ImportDocumentUrlRequest request)
    {
        var caseExists = await _db.Cases.AnyAsync(c => c.Id == caseId);
        if (!caseExists)
        {
            return NotFound();
        }

        ImportedDocument imported;
        try
        {
            imported = await _importService.ImportAsync(caseId, request.Url);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Kunne ikke hente dokument fra URL: {ex.Message}" });
        }

        var document = new Document
        {
            CaseId = caseId,
            FileName = imported.FileName,
            StoredFileName = Path.GetFileName(imported.FileName),
            ContentType = imported.ContentType,
            SizeBytes = imported.SizeBytes,
            UploadedByUserId = GetCallerUserId(),
            StoragePath = imported.StoragePath,
        };
        _db.Documents.Add(document);

        _db.AuditEvents.Add(new AuditEvent
        {
            EntityType = nameof(Case),
            EntityId = caseId,
            UserId = GetCallerUserId(),
            Action = "DokumentImportertFraUrl",
            Details = $"Importerte dokument fra {request.Url}.",
        });

        await _db.SaveChangesAsync();

        var created = await _db.Documents.Include(d => d.UploadedByUser).FirstAsync(d => d.Id == document.Id);
        return Ok(DocumentDto.From(created));
    }

    private int GetCallerUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
