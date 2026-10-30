using System.Diagnostics;

namespace Saksbehandling.Api.Services;

/// <summary>
/// Renders an HTML preview of an office document (docx/txt/md/…) using
/// pandoc, so it can be shown inline instead of forcing a download.
/// </summary>
public class DocumentConversionService
{
    public async Task<string> ConvertToHtmlPreviewAsync(string inputPath, string originalFileName)
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"preview-{Guid.NewGuid():N}.html");

        var command = $"pandoc \"{inputPath}\" -o \"{outputPath}\" --metadata title=\"{originalFileName}\" 2>/dev/null";

        var psi = new ProcessStartInfo
        {
            FileName = "/bin/sh",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        psi.ArgumentList.Add("-c");
        psi.ArgumentList.Add(command);

        using var process = Process.Start(psi)!;
        await process.WaitForExitAsync();

        if (!File.Exists(outputPath))
        {
            return "<p>Forhåndsvisning er ikke tilgjengelig for denne filen.</p>";
        }

        var html = await File.ReadAllTextAsync(outputPath);
        File.Delete(outputPath);
        return html;
    }
}
