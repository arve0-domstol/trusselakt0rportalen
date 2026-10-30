using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Saksbehandling.Api.Data;
using Saksbehandling.Api.Dtos;
using Saksbehandling.Api.Models;
using Saksbehandling.Api.Services;

namespace Saksbehandling.Api.Controllers;

[ApiController]
[Route("api/v2/cases/{caseId:int}")]
[Authorize]
public class CaseExportController : ControllerBase
{
    private readonly CaseExportService _exportService;
    private readonly CasePdfExportService _pdfExportService;
    private readonly AppDbContext _db;

    public CaseExportController(CaseExportService exportService, CasePdfExportService pdfExportService, AppDbContext db)
    {
        _exportService = exportService;
        _pdfExportService = pdfExportService;
        _db = db;
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export(int caseId)
    {
        var json = await _exportService.ExportAsync(caseId);
        if (json is null)
        {
            return NotFound();
        }

        return File(Encoding.UTF8.GetBytes(json), "application/json", $"sak-{caseId}-eksport.json");
    }

    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportPdf(int caseId)
    {
        var pdf = await _pdfExportService.RenderCasePdfAsync(caseId);
        if (pdf is null)
        {
            return NotFound();
        }

        return File(pdf, "application/pdf", $"sak-{caseId}.pdf");
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import(int caseId, ImportCaseHistoryRequest request)
    {
        var c = await _db.Cases.FindAsync(caseId);
        if (c is null)
        {
            return NotFound();
        }

        CaseExportBundle? bundle;
        try
        {
            bundle = _exportService.Import(request.Json);
        }
        catch (Newtonsoft.Json.JsonException)
        {
            return BadRequest(new { message = "Ugyldig eksportfil." });
        }

        if (bundle is null)
        {
            return BadRequest(new { message = "Ugyldig eksportfil." });
        }

        foreach (var entry in bundle.History)
        {
            _db.AuditEvents.Add(new AuditEvent
            {
                EntityType = nameof(Case),
                EntityId = caseId,
                Action = "Historikk importert",
                Details = entry switch
                {
                    CommentHistoryEntry ce => $"Kommentar: {ce.Text}",
                    AssignmentHistoryEntry ae => $"Tildeling: {ae.AssignedTo}",
                    AuditHistoryEntry ad => ad.Action,
                    _ => entry.GetType().Name,
                },
                Timestamp = entry.Timestamp,
            });
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = "Historikk importert.", count = bundle.History.Count });
    }
}
