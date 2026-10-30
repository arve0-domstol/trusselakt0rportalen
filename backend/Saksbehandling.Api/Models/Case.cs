namespace Saksbehandling.Api.Models;

/// <summary>Sak. The central entity - a case to be investigated and resolved.</summary>
public class Case
{
    public int Id { get; set; }

    /// <summary>Saksnummer, e.g. "SAK-2026-00042". Human-facing, distinct from Id.</summary>
    public string CaseNumber { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    public int? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }

    public CaseStatus Status { get; set; } = CaseStatus.Ny;
    public CasePriority Priority { get; set; } = CasePriority.Normal;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Frist - deadline for handling the case.</summary>
    public DateTime? Deadline { get; set; }

    public DateTime? ClosedAt { get; set; }

    public int? ApprovedByUserId { get; set; }
    public User? ApprovedByUser { get; set; }

    public ICollection<CaseAssignment> AssignmentHistory { get; set; } = new List<CaseAssignment>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
