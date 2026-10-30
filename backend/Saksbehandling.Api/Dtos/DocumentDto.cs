using Saksbehandling.Api.Models;

namespace Saksbehandling.Api.Dtos;

public class DocumentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }

    public static DocumentDto From(Document d) => new()
    {
        Id = d.Id,
        FileName = d.FileName,
        ContentType = d.ContentType,
        SizeBytes = d.SizeBytes,
        UploadedByName = d.UploadedByUser?.FullName ?? string.Empty,
        UploadedAt = d.UploadedAt,
    };
}

public class ImportDocumentUrlRequest
{
    public string Url { get; set; } = string.Empty;
}
