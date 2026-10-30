namespace Saksbehandling.Api.Models;

/// <summary>One entry in a case's exported/imported history timeline.</summary>
public abstract class CaseHistoryEntry
{
    public DateTime Timestamp { get; set; }
}

public class CommentHistoryEntry : CaseHistoryEntry
{
    public string Author { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
}

public class AssignmentHistoryEntry : CaseHistoryEntry
{
    public string AssignedTo { get; set; } = string.Empty;
    public string AssignedBy { get; set; } = string.Empty;
    public string? Note { get; set; }
}

public class AuditHistoryEntry : CaseHistoryEntry
{
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
}
