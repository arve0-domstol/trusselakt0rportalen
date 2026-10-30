using Microsoft.AspNetCore.Mvc;
using Saksbehandling.Api.Data;
using Saksbehandling.Api.FileStorage;
using Saksbehandling.Api.Services;

namespace Saksbehandling.Api.Controllers;

[ApiController]
[Route("api/documents")]
public class DocumentsController : ControllerBase
{
    private static readonly HashSet<string> InlinePreviewableContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "text/plain",
        "text/html",
    };

    private readonly AppDbContext _db;
    private readonly IFileStorage _fileStorage;
    private readonly DocumentConversionService _conversionService;

    public DocumentsController(AppDbContext db, IFileStorage fileStorage, DocumentConversionService conversionService)
    {
        _db = db;
        _fileStorage = fileStorage;
        _conversionService = conversionService;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Download(int id)
    {
        var document = await _db.Documents.FindAsync(id);
        if (document is null)
        {
            return NotFound();
        }

        var stream = await _fileStorage.OpenReadAsync(document.StoragePath);
        if (stream is null)
        {
            return NotFound();
        }

        var isInline = document.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
            || InlinePreviewableContentTypes.Contains(document.ContentType);

        Response.Headers.ContentDisposition = isInline
            ? $"inline; filename=\"{document.FileName}\""
            : $"attachment; filename=\"{document.FileName}\"";

        return File(stream, document.ContentType);
    }

    [HttpGet("{id:int}/preview")]
    public async Task<IActionResult> Preview(int id)
    {
        var document = await _db.Documents.FindAsync(id);
        if (document is null)
        {
            return NotFound();
        }

        if (document.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
            || document.ContentType == "application/pdf")
        {
            return await Download(id);
        }

        var absolutePath = _fileStorage.GetAbsolutePath(document.StoragePath);
        if (!System.IO.File.Exists(absolutePath))
        {
            return NotFound();
        }

        var html = await _conversionService.ConvertToHtmlPreviewAsync(absolutePath, document.FileName);
        return Content(html, "text/html");
    }
}
