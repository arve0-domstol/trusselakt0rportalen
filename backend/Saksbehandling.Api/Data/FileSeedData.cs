using Microsoft.EntityFrameworkCore;

namespace Saksbehandling.Api.Data;

/// <summary>
/// The database seed creates <c>Document</c> rows before the file-storage
/// pipeline exists to give them real content. This fills in a small
/// placeholder file for any seeded document that doesn't have one on disk
/// yet, so downloads/previews have something to actually open. Safe to run
/// on every startup - it only writes a file when one is missing.
/// </summary>
public static class FileSeedData
{
    private static readonly byte[] TinyPng = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=");

    private const string TinyPdf =
        "%PDF-1.4\n1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj\n2 0 obj<</Type/Pages/Kids[3 0 R]/Count 1>>endobj\n3 0 obj<</Type/Page/Parent 2 0 R/MediaBox[0 0 200 200]>>endobj\ntrailer<</Root 1 0 R>>\n%%EOF";

    public static async Task EnsurePlaceholderFilesAsync(AppDbContext db, string storageRoot)
    {
        var documents = await db.Documents.ToListAsync();

        foreach (var doc in documents)
        {
            var fullPath = Path.Combine(storageRoot, doc.StoragePath);
            if (File.Exists(fullPath))
            {
                continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            if (doc.ContentType == "image/png")
            {
                await File.WriteAllBytesAsync(fullPath, TinyPng);
            }
            else if (doc.ContentType == "application/pdf")
            {
                await File.WriteAllTextAsync(fullPath, TinyPdf);
            }
            else
            {
                await File.WriteAllTextAsync(
                    fullPath,
                    $"Plassholderinnhold for {doc.FileName}.\n\nOpprinnelig lastet opp {doc.UploadedAt:yyyy-MM-dd}.");
            }
        }
    }
}
