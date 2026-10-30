namespace Saksbehandling.Api.Models;

/// <summary>
/// Assignment/reassignment history for a case, distinct from
/// Case.AssignedToUserId (which holds only the *current* assignee).
/// </summary>
public class CaseAssignment
{
    public int Id { get; set; }

    public int CaseId { get; set; }
    public Case? Case { get; set; }

    public int AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }

    public int AssignedByUserId { get; set; }
    public User? AssignedByUser { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public string? Note { get; set; }
}
