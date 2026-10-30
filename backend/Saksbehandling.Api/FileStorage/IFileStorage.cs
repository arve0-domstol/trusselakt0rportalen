namespace Saksbehandling.Api.FileStorage;

/// <summary>
/// Abstraction over where uploaded case documents actually live on disk, so
/// the storage implementation can be swapped without touching controllers.
/// </summary>
public interface IFileStorage
{
    /// <summary>Saves the given content under the case's folder and returns
    /// the path stored on the <c>Document</c> row (relative to the storage
    /// root).</summary>
    Task<string> SaveAsync(int caseId, string fileName, Stream content);

    Task<Stream?> OpenReadAsync(string relativePath);

    string GetAbsolutePath(string relativePath);
}
