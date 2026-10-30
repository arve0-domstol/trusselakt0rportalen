namespace Saksbehandling.Api.Models;

/// <summary>
/// Vedlegg. A file attached to a case, either uploaded directly or imported
/// from a URL. The upload/download pipeline itself is built in a later
/// phase - this entity only carries metadata for now.
/// </summary>
public class Document
{
    public int Id { get; set; }

    public int CaseId { get; set; }
    public Case? Case { get; set; }

    /// <summary>Original filename as supplied by the uploader.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Server-side stored filename/path segment.</summary>
    public string StoredFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }

    public int UploadedByUserId { get; set; }
    public User? UploadedByUser { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Relative path under the storage volume, e.g. "cases/42/...".</summary>
    public string StoragePath { get; set; } = string.Empty;
}
