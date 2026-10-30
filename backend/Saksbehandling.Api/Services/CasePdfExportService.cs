using System.Diagnostics;
using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Saksbehandling.Api.Data;

namespace Saksbehandling.Api.Services;

/// <summary>
/// Renders a case — its key fields plus the full comment thread — to a PDF so
/// a saksbehandler can download or print a case summary. The case is laid out
/// as an HTML document and converted to PDF with wkhtmltopdf.
/// </summary>
public class CasePdfExportService
{
    private readonly AppDbContext _db;

    public CasePdfExportService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<byte[]?> RenderCasePdfAsync(int caseId)
    {
        var c = await _db.Cases
            .Include(x => x.Customer)
            .Include(x => x.Department)
            .Include(x => x.AssignedToUser)
            .Include(x => x.Comments).ThenInclude(cm => cm.AuthorUser)
            .FirstOrDefaultAsync(x => x.Id == caseId);
        if (c is null)
        {
            return null;
        }

        var html = BuildHtml(c);

        var workDir = Path.Combine(Path.GetTempPath(), $"case-pdf-{Guid.NewGuid():N}");
        Directory.CreateDirectory(workDir);
        var htmlPath = Path.Combine(workDir, "case.html");
        var pdfPath = Path.Combine(workDir, "case.pdf");

        try
        {
            await File.WriteAllTextAsync(htmlPath, html, Encoding.UTF8);

            var psi = new ProcessStartInfo
            {
                FileName = "wkhtmltopdf",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            // Keep the console output quiet and force UTF-8 so Norwegian
            // characters in the case text survive into the PDF.
            psi.ArgumentList.Add("--quiet");
            psi.ArgumentList.Add("--encoding");
            psi.ArgumentList.Add("utf-8");
            psi.ArgumentList.Add(htmlPath);
            psi.ArgumentList.Add(pdfPath);

            using (var process = Process.Start(psi)!)
            {
                await process.WaitForExitAsync();
            }

            if (!File.Exists(pdfPath))
            {
                return null;
            }

            return await File.ReadAllBytesAsync(pdfPath);
        }
        finally
        {
            try
            {
                Directory.Delete(workDir, recursive: true);
            }
            catch
            {
                // Best-effort cleanup of the scratch directory.
            }
        }
    }

    private static string BuildHtml(Models.Case c)
    {
        static string E(string? s) => WebUtility.HtmlEncode(s ?? string.Empty);

        var sb = new StringBuilder();
        sb.Append("<!doctype html><html lang=\"no\"><head><meta charset=\"utf-8\">");
        sb.Append("<style>");
        sb.Append("body{font-family:'Helvetica',Arial,sans-serif;color:#1a1a1a;margin:40px;}");
        sb.Append("h1{font-size:20px;margin-bottom:4px;}");
        sb.Append("h2{font-size:15px;margin-top:28px;border-bottom:1px solid #ccc;padding-bottom:4px;}");
        sb.Append(".meta{font-size:12px;color:#444;}");
        sb.Append(".meta dt{font-weight:bold;float:left;width:120px;clear:left;}");
        sb.Append(".meta dd{margin:0 0 4px 130px;}");
        sb.Append(".comment{margin:12px 0;padding:8px 12px;border-left:3px solid #ddd;}");
        sb.Append(".comment .byline{font-size:11px;color:#666;margin-bottom:4px;}");
        sb.Append("</style></head><body>");

        sb.Append($"<h1>{E(c.CaseNumber)} — {E(c.Title)}</h1>");

        sb.Append("<dl class=\"meta\">");
        sb.Append($"<dt>Status</dt><dd>{E(c.Status.ToString())}</dd>");
        sb.Append($"<dt>Prioritet</dt><dd>{E(c.Priority.ToString())}</dd>");
        sb.Append($"<dt>Kunde</dt><dd>{E(c.Customer?.Name)}</dd>");
        sb.Append($"<dt>Avdeling</dt><dd>{E(c.Department?.Name)}</dd>");
        sb.Append($"<dt>Saksbehandler</dt><dd>{E(c.AssignedToUser?.FullName ?? "—")}</dd>");
        sb.Append($"<dt>Opprettet</dt><dd>{E(c.CreatedAt.ToString("yyyy-MM-dd HH:mm"))}</dd>");
        sb.Append("</dl>");

        sb.Append("<h2>Beskrivelse</h2>");
        sb.Append($"<p>{E(c.Description)}</p>");

        sb.Append("<h2>Kommentarer</h2>");
        if (c.Comments.Count == 0)
        {
            sb.Append("<p><em>Ingen kommentarer.</em></p>");
        }
        else
        {
            foreach (var cm in c.Comments.OrderBy(x => x.CreatedAt))
            {
                var author = E(cm.AuthorUser?.FullName ?? cm.AuthorUserId.ToString());
                var when = E(cm.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
                sb.Append("<div class=\"comment\">");
                sb.Append($"<div class=\"byline\">{author} · {when}{(cm.IsInternal ? " · intern" : string.Empty)}</div>");
                // Comment bodies may contain light formatting entered by
                // saksbehandlere, so they are placed into the report as-is to
                // preserve that formatting in the printed summary.
                sb.Append($"<div class=\"body\">{cm.Text}</div>");
                sb.Append("</div>");
            }
        }

        sb.Append("</body></html>");
        return sb.ToString();
    }
}
