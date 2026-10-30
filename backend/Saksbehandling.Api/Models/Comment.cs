namespace Saksbehandling.Api.Models;

/// <summary>Kommentar. A comment on a case, optionally internal-only.</summary>
public class Comment
{
    public int Id { get; set; }

    public int CaseId { get; set; }
    public Case? Case { get; set; }

    public int AuthorUserId { get; set; }
    public User? AuthorUser { get; set; }

    public string Text { get; set; } = string.Empty;

    /// <summary>Intern kommentar - not intended to be visible to the customer.</summary>
    public bool IsInternal { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
