using Saksbehandling.Api.Models;

namespace Saksbehandling.Api.Dtos;

public class CaseExportBundle
{
    public int CaseId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public List<CaseHistoryEntry> History { get; set; } = new();
}

public class ImportCaseHistoryRequest
{
    public string Json { get; set; } = string.Empty;
}
