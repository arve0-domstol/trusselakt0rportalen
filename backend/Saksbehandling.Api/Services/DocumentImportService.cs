using Saksbehandling.Api.FileStorage;

namespace Saksbehandling.Api.Services;

public class ImportedDocument
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string StoragePath { get; set; } = string.Empty;
}

/// <summary>
/// Fetches a document from a URL supplied by the caller and stores it as a
/// case attachment, for the "Importer dokument fra URL" feature.
/// </summary>
public class DocumentImportService
{
    private readonly HttpClient _httpClient;
    private readonly IFileStorage _fileStorage;
    private readonly IConfiguration _configuration;

    public DocumentImportService(HttpClient httpClient, IFileStorage fileStorage, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _fileStorage = fileStorage;
        _configuration = configuration;
    }

    public async Task<ImportedDocument> ImportAsync(int caseId, string url)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("X-Internal-Api-Key", _configuration["InternalApi:ApiKey"]);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        var fileName = Path.GetFileName(new Uri(url).LocalPath);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = "importert-dokument";
        }

        var bytes = await response.Content.ReadAsByteArrayAsync();
        using var contentStream = new MemoryStream(bytes);
        var storagePath = await _fileStorage.SaveAsync(caseId, fileName, contentStream);

        return new ImportedDocument
        {
            FileName = fileName,
            ContentType = contentType,
            SizeBytes = bytes.LongLength,
            StoragePath = storagePath,
        };
    }
}
