namespace Saksbehandling.Api.FileStorage;

public class LocalDiskFileStorage : IFileStorage
{
    private readonly string _rootPath;

    public LocalDiskFileStorage(IConfiguration configuration)
    {
        _rootPath = configuration["Storage:RootPath"] ?? "/app/storage";
    }

    public async Task<string> SaveAsync(int caseId, string fileName, Stream content)
    {
        var caseFolder = Path.Combine(_rootPath, "cases", caseId.ToString());
        Directory.CreateDirectory(caseFolder);

        // Keep the uploader's own filename so downloads/links stay
        // recognizable rather than showing a generated ID.
        var destinationPath = Path.Combine(caseFolder, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

        await using var fileStream = File.Create(destinationPath);
        await content.CopyToAsync(fileStream);

        return Path.Combine("cases", caseId.ToString(), fileName);
    }

    public Task<Stream?> OpenReadAsync(string relativePath)
    {
        var fullPath = Path.Combine(_rootPath, relativePath);
        if (!File.Exists(fullPath))
        {
            return Task.FromResult<Stream?>(null);
        }

        return Task.FromResult<Stream?>(File.OpenRead(fullPath));
    }

    public string GetAbsolutePath(string relativePath) => Path.Combine(_rootPath, relativePath);
}
